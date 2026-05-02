namespace Ppu.Domain;

public sealed class RawReadResult
{
    public required DateTime TimestampUtc { get; init; }
    public required bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public required ushort StartAddress { get; init; }
    public required ushort RegisterCount  { get; init; } 
    public required ushort FunctionCode { get; init; }
    public required ushort[] Registers { get; init; } = [];  //Array.Empty<ushort>();
    public required int DurationsMs { get; init; }

}