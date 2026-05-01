namespace Ppu.Domain;

public sealed class RawReadResult
{
    public DateTimeOffset TimestampUtc { get; init; }
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int FunctionCode { get; init; } = (int)ModbusFunctionCode.ReadHoldingRegisters;
    public ushort StartAddress { get; init; } = 0;
    public ushort[] Registers { get; init; } = [];  //Array.Empty<ushort>();
    public int DurationsMs { get; init; }

}