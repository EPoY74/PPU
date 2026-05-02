using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Xunit;

namespace Ppu.Tests.E2E;

public sealed class PpuApiE2eTests
{
    private const string ApplicationName = "PPU";
    private const string ApiVersion = "0.1.1";
    private const int ExpectedFunctionCode = 4;

    private static readonly ushort[] ExpectedRegisters = [101, 102, 103, 104];
    private static readonly Uri BaseUri = new(
        Environment.GetEnvironmentVariable("PPU_E2E_BASE_URL") ?? "http://localhost:5055");

    [Fact(DisplayName = "E2E: /health returns ok when API container is running")]
    public async Task Health_ReturnsOk_WhenApiContainerIsRunning()
    {
        using var httpClient = CreateHttpClient();

        var health = await WaitForJsonAsync<HealthResponse>(httpClient, "/health");

        Assert.Equal(ApplicationName, health.Application);
        Assert.Equal("ok", health.Status);
        Assert.NotEqual(default, health.Utc);
    }

    [Fact(DisplayName = "E2E: / returns service metadata and endpoint links")]
    public async Task Root_ReturnsServiceMetadataAndEndpointLinks()
    {
        using var httpClient = CreateHttpClient();
        await WaitForApiAsync(httpClient);

        var root = await httpClient.GetFromJsonAsync<RootResponse>("/");

        Assert.NotNull(root);
        Assert.Equal(ApplicationName, root.Application);
        Assert.Equal(ApiVersion, root.Version);
        Assert.Equal("Running", root.Status);
        Assert.EndsWith("/health", root.Endpoints.HealthEndpoint);
        Assert.EndsWith("/last-read", root.Endpoints.LastReadEndpoint);
        Assert.EndsWith("/openapi/v1.json", root.Endpoints.OpenApiEndpoint);
        Assert.EndsWith("/history", root.Endpoints.HistoryEndpoint);
    }

    [Fact(DisplayName = "E2E: OpenAPI document is published")]
    public async Task OpenApi_ReturnsDocument()
    {
        using var httpClient = CreateHttpClient();
        await WaitForApiAsync(httpClient);

        using var response = await httpClient.GetAsync("/openapi/v1.json");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"title\": \"PPU API\"", body);
        Assert.Contains("\"/last-read\"", body);
        Assert.Contains("\"/history\"", body);
    }

    [Fact(DisplayName = "E2E: /last-read returns FC04 registers from Dockerized PLC simulator")]
    public async Task LastRead_ReturnsSuccessfulRead_FromDockerizedSimulator()
    {
        using var httpClient = CreateHttpClient();
        await WaitForApiAsync(httpClient);

        var lastRead = await WaitForLastReadAsync(httpClient);

        Assert.True(lastRead.IsSuccess);
        Assert.Null(lastRead.ErrorMessage);
        Assert.Equal(ExpectedFunctionCode, lastRead.FunctionCode);
        Assert.Equal(ExpectedRegisters, lastRead.Registers);
    }

    [Fact(DisplayName = "E2E: /history returns persisted PLC read records")]
    public async Task History_ReturnsPersistedReadRecords()
    {
        using var httpClient = CreateHttpClient();
        await WaitForApiAsync(httpClient);
        await WaitForLastReadAsync(httpClient);

        var history = await WaitForHistoryAsync(httpClient);
        var latestSuccessfulRead = history.FirstOrDefault(x => x.IsSuccess);

        Assert.NotNull(latestSuccessfulRead);
        Assert.Equal(ExpectedFunctionCode, latestSuccessfulRead.FunctionCode);
        Assert.Equal(0, latestSuccessfulRead.StartAddress);
        Assert.Equal(ExpectedRegisters.Length, latestSuccessfulRead.RegisterCount);
        Assert.Equal(ExpectedRegisters, ReadRegistersJson(latestSuccessfulRead.RegistersJson));
    }

    private static HttpClient CreateHttpClient() => new()
    {
        BaseAddress = BaseUri,
        Timeout = TimeSpan.FromSeconds(5)
    };

    private static async Task WaitForApiAsync(HttpClient httpClient)
    {
        await WaitUntilAsync(async () =>
        {
            using var response = await httpClient.GetAsync("/health");
            return response.StatusCode == HttpStatusCode.OK;
        }, TimeSpan.FromSeconds(30));
    }

    private static async Task<TResponse> WaitForJsonAsync<TResponse>(
        HttpClient httpClient,
        string requestUri)
        where TResponse : class
    {
        TResponse? result = null;

        await WaitUntilAsync(async () =>
        {
            using var response = await httpClient.GetAsync(requestUri);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            result = await response.Content.ReadFromJsonAsync<TResponse>();
            return result is not null;
        }, TimeSpan.FromSeconds(30));

        return result ?? throw new InvalidOperationException(
            $"Endpoint '{requestUri}' did not return a JSON response.");
    }

    private static async Task<LastReadResponse> WaitForLastReadAsync(HttpClient httpClient)
    {
        LastReadResponse? lastRead = null;

        await WaitUntilAsync(async () =>
        {
            using var response = await httpClient.GetAsync("/last-read");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            lastRead = await response.Content.ReadFromJsonAsync<LastReadResponse>();

            return lastRead is { IsSuccess: true };
        }, TimeSpan.FromSeconds(30));

        return lastRead ?? throw new InvalidOperationException("Last read response was not returned.");
    }

    private static async Task<IReadOnlyCollection<HistoryItem>> WaitForHistoryAsync(HttpClient httpClient)
    {
        IReadOnlyCollection<HistoryItem>? history = null;

        await WaitUntilAsync(async () =>
        {
            using var response = await httpClient.GetAsync("/history");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            history = await response.Content.ReadFromJsonAsync<HistoryItem[]>();
            return history?.Any(x => x.IsSuccess) == true;
        }, TimeSpan.FromSeconds(30));

        return history ?? throw new InvalidOperationException("History response was not returned.");
    }

    private static ushort[] ReadRegistersJson(string registersJson)
    {
        return JsonSerializer.Deserialize<ushort[]>(registersJson)
               ?? throw new InvalidOperationException("History registers JSON was not an array.");
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var delay = pollInterval ?? TimeSpan.FromMilliseconds(250);

        while (DateTimeOffset.UtcNow - startedAt < timeout)
        {
            try
            {
                if (await condition())
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The compose services may still be starting or restarting.
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException($"Condition was not met within {timeout}.");
    }

    private sealed record LastReadResponse(
        DateTimeOffset TimestampUtc,
        bool IsSuccess,
        string? ErrorMessage,
        int FunctionCode,
        ushort[] Registers,
        int DurationsMs);

    private sealed record HealthResponse(
        string Application,
        string Status,
        DateTimeOffset Utc);

    private sealed record RootResponse(
        string Application,
        string Version,
        string Status,
        string Description,
        EndpointLinks Endpoints);

    private sealed record EndpointLinks(
        string HealthEndpoint,
        string LastReadEndpoint,
        string OpenApiEndpoint,
        string HistoryEndpoint);

    private sealed record HistoryItem(
        long Id,
        Guid AppRunId,
        DateTimeOffset TimestampUtc,
        bool IsSuccess,
        string? ErrorMessage,
        ushort FunctionCode,
        ushort StartAddress,
        ushort RegisterCount,
        string RegistersJson,
        int DurationMs);
}
