using Ppu.Domain;

namespace Ppu.Config;

public sealed class PlcReaderOptions
{
    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 502;
    public byte UnitId { get; init; } = 1;
    public ModbusFunctionCode FunctionCode { get; init; } = ModbusFunctionCode.ReadHoldingRegisters;
    public ushort StartAddress { get; init; } = 0;
    public ushort RegisterCount { get; init; } = 2;
    public int PollIntervalSecond { get; init; } = 5;

}