using Microsoft.Extensions.Options;
using Ppu.Config;

namespace Ppu.Services;

public sealed class  PollingWorker : BackgroundService
{
    private readonly IPlcReader _plcReader;
    private readonly LastReadStore _lastReadStore;
    private readonly ILogger<PollingWorker> _logger;
    private readonly PlcReaderOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public PollingWorker(
            IPlcReader plcReader,
            LastReadStore lastReadStore,
            IOptions<PlcReaderOptions> options,
            IServiceScopeFactory scopeFactory,
            ILogger<PollingWorker> logger
        )
    {
        _plcReader = plcReader;
        _lastReadStore = lastReadStore;
        _logger = logger;
        _options = options.Value;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PPU Logging Started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _plcReader.RawReadAsync(stoppingToken);
                _lastReadStore.Set(result);
                using var scope = _scopeFactory.CreateScope();
                var historyWriter = scope.ServiceProvider.GetRequiredService<IRawReadHistoryWriter>();
                await historyWriter.SaveAsync(result, stoppingToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                            "Read succsessfull at {TimestampUtc}, FC:{FunctionCode}, duration: {DurationMs}ms, Registers: {Registers}",
                            result.TimestampUtc,
                            (int)_options.FunctionCode,
                            result.DurationsMs,
                            string.Join(", ", result.Registers)
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "read failed  at {TimestampUtc}, error: {ErrorMessage}",
                        result.TimestampUtc,
                        result.ErrorMessage
                        );
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unpandled error during PLC read");
            }
            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                stoppingToken);
        }
        _logger.LogInformation("PPU Logging Stopped");
    }

}