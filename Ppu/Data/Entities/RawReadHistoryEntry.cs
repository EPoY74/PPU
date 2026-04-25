using System.ComponentModel.DataAnnotations;

namespace Ppu.Data.Entities;

public sealed class RawReadHistoryEntry
{
    public long Id { get; set; }
    public Guid AppRunId { get; set; }
    public DateTimeOffset TimeStampUtc { get; set; }
    public bool IsSuccess { get; set; }
    [StringLength(512)]
    public string? ErrorMessage {  get; set; }
    public int FunctionCode { get; set; }
    public ushort StartAddress { get; set; }
    public ushort RegisterCouunt { get; set; }
    public ushort StopAddress { get; set; }
    [StringLength(270)]
    public string RegistersJson { get; set; } = "[]";
    public int Duration { get; set; }
}
