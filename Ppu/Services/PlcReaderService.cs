using System.Diagnostics;

using Microsoft.Extensions.Options;

using FluentModbus;

using Ppu.Config;
using Ppu.Domain;


namespace Ppu.Services;

public sealed class PlcReaderService : IPlcReader
{
    private readonly PlcReaderOptions _options;
    private readonly ILogger _logger;
    private string _plcEndpoint = "";


    public PlcReaderService(
        IOptions<PlcReaderOptions> options,
        ILogger<PlcReaderService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RawReadResult> RawReadAsync(CancellationToken cancellationToken)
    {
        _plcEndpoint = _options.Host + ":" + _options.Port;

        using var client = new ModbusTcpClient();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            client.Connect(_plcEndpoint, ModbusEndianness.BigEndian); // LittleEndian        
            
            var readRegisters  = _options.FunctionCode switch
            {
               Ppu.Domain.ModbusFunctionCode.ReadHoldingRegisters => 
                   client.ReadHoldingRegisters<ushort>(
                   _options.UnitId,
                   _options.StartAddress,
                   _options.RegisterCount),

               Ppu.Domain.ModbusFunctionCode.ReadInputRegisters =>
                   client.ReadInputRegisters<ushort>(
                   _options.UnitId,
                   _options.StartAddress,
                   _options.RegisterCount),
               
                _ => throw new NotSupportedException(
                    $"FunctionCode '{_options.FunctionCode}' is not supported. Supported codes: FC03, FC04.")
            };


                var _registers = readRegisters.ToArray();
                
               
            await Task.Yield();
            stopwatch.Stop();
            return new RawReadResult
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                IsSuccess = true,
                FunctionCode = (int)_options.FunctionCode,
                Registers = _registers,
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
                ErrorMessage = ex.Message,
                FunctionCode = (int)_options.FunctionCode,
                DurationsMs = (int)stopwatch.ElapsedMilliseconds

            };
        }

    }
}