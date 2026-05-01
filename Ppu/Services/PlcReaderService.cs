using System.Diagnostics;
using System.Net.Sockets;

using Microsoft.Extensions.Options;

using FluentModbus;

using Ppu.Config;
using Ppu.Domain;


namespace Ppu.Services;

public sealed class PlcReaderService : IPlcReader
{
    private readonly PlcReaderOptions _options;

    public PlcReaderService(
        IOptions<PlcReaderOptions> options)
    {
        _options = options.Value;
    }

    public async Task<RawReadResult> RawReadAsync(CancellationToken cancellationToken)
    {

        using var client = new ModbusTcpClient();
        using var tcpClient = new TcpClient();
        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectCts.CancelAfter(_options.ConnectTimeoutMilliseconds);
        using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readCts.CancelAfter(_options.ReadTimeoutMilliseconds);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await tcpClient.ConnectAsync(_options.Host, _options.Port, connectCts.Token);
            client.Initialize(tcpClient, ModbusEndianness.BigEndian);

            var readRegisters = _options.FunctionCode switch
            {
                Domain.ModbusFunctionCode.ReadHoldingRegisters =>
                    await client.ReadHoldingRegistersAsync<ushort>(
                        _options.UnitId,
                        _options.StartAddress,
                        _options.RegisterCount,
                        readCts.Token),

                Domain.ModbusFunctionCode.ReadInputRegisters =>
                    await client.ReadInputRegistersAsync<ushort>(
                        _options.UnitId,
                        _options.StartAddress,
                        _options.RegisterCount,
                        readCts.Token),

                _ => throw new NotSupportedException(
                    $"FunctionCode '{_options.FunctionCode}' is not supported. Supported codes: FC03, FC04.")
            };
            stopwatch.Stop();
            return new RawReadResult
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                IsSuccess = true,
                ErrorMessage = null,
                StartAddress = _options.StartAddress,
                Registers = readRegisters.ToArray(),
                FunctionCode = (int)_options.FunctionCode,
                DurationsMs = (int)stopwatch.ElapsedMilliseconds

            };

            
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                throw;

            if (connectCts.IsCancellationRequested)
            {
                stopwatch.Stop();
                var connectionTimeout = _options.ConnectTimeoutMilliseconds;

                return new RawReadResult
                {
                    TimestampUtc = DateTimeOffset.UtcNow,
                    IsSuccess = false,
                    StartAddress =  _options.StartAddress,
                    ErrorMessage = $"Connection operation timed out after {connectionTimeout} milliseconds",
                    FunctionCode = (int)_options.FunctionCode,
                    DurationsMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
            
            if (readCts.IsCancellationRequested)
            {
                stopwatch.Stop();
                var readTimeout = _options.ReadTimeoutMilliseconds;
                
                return new RawReadResult
                {
                    TimestampUtc = DateTimeOffset.UtcNow,
                    IsSuccess = false,
                    StartAddress =  _options.StartAddress,
                    ErrorMessage = $"Read operation timed out after {readTimeout} milliseconds",
                    FunctionCode = (int)_options.FunctionCode,
                    DurationsMs = (int)stopwatch.ElapsedMilliseconds

                };
            }
            
            stopwatch.Stop();
            return new RawReadResult
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                IsSuccess = false,
                StartAddress =  _options.StartAddress,
                ErrorMessage = $"Unknown error",
                FunctionCode = (int)_options.FunctionCode,
                DurationsMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new RawReadResult
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                IsSuccess = false,
                StartAddress =  _options.StartAddress,
                ErrorMessage = ex.Message,
                FunctionCode = (int)_options.FunctionCode,
                DurationsMs = (int)stopwatch.ElapsedMilliseconds

            };
        }
        
    }
}
