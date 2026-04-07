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
    private string _plcEndpoint;


    public PlcReaderService(
        IOptions<PlcReaderOptions> options,
        ILogger<PlcReaderService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RawReadResult> RawReadAsync(CanсelationToken cancellationToken)
    {
        _plcEndpoint = _options.Host + ":" + _options.Port;

        using var client = new ModbusTcpClient();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            client.Connect(_plcEndpoint, ModbusEndianness.BigEndian); // LittleEndian        
            var registers = client.ReadHoldingRegisters<ushort>(
                _options.UnitId,
                _options.StartAddress,
                _options.RegisterCount);
            await Task.Yield();
            stopwatch.Stop();
        }
        catch (Exception ex)
        {
            _plcEndpoint(
                )
        }

    }
}