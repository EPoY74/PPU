using Ppu.Config;
using Ppu.Dtos;
using Ppu.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PlcReaderOptions>(
    builder.Configuration.GetSection("PlcReader"));

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "PPU API";
        document.Info.Version = "0.1.1";
        document.Info.Description = "Simple Modbus TCP PLC polling utility with HTTP API.";

        return Task.CompletedTask;
    });
});

builder.Services.AddSingleton<LastReadStore>();
builder.Services.AddSingleton<IPlcReader, PlcReaderService>();
builder.Services.AddHostedService<PollingWorker>();


var app = builder.Build();
app.MapOpenApi();


app.MapGet("/", static () =>
{
    var rootResponse = new RootResponseDto(
        Application: "PPU",
        Version: "0.1.1",
        Status: "Running",
        Description: "Simple modbus TCP PLC polling utility with HTTP API.",
        Endpoints: new EndpointLinksDto(
            "/health",
            "/last-read",
            "/openapi/v1.json"
        )
    );
    return Results.Ok(rootResponse);
})
.WithSummary("Get application info")
.WithDescription("Returns basic information about the PPU service and available endpoints.")
.Produces<RootResponseDto>(StatusCodes.Status200OK);

app.MapGet("/health", () =>
{
    var dto = new HealthResponseDto(
        "PPU", 
        "ok", 
        DateTimeOffset.UtcNow);
    
    return Results.Ok(dto);
})
.WithSummary("Get service health status")
.WithDescription("Returns a simple health response for the PPU API.")
.Produces<HealthResponseDto>(StatusCodes.Status200OK);


app.MapGet("/last-read", (LastReadStore store) =>
{
    var result = store.Get();
    if (result is null)
    {
        return Results.NotFound(new DataReadErrorDto("No read yet."));
    }
    return Results.Ok(new LastReadResponseDto(
        result.TimestampUtc,
        result.IsSuccess,
        result.ErrorMessage,
        result.FunctionCode,
        result.Registers,
        result.DurationsMs
    ));
})
.WithSummary("Get last PLC read result")
.WithDescription("Returns the latest read result stored in memory.")
.Produces<LastReadResponseDto>(StatusCodes.Status200OK)
.Produces<DataReadErrorDto>(StatusCodes.Status404NotFound);


app.Run();

