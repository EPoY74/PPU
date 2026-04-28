using System.ComponentModel.DataAnnotations;
using Ppu.Domain;

namespace Ppu.Config;

public sealed class PlcReaderOptions
{
    [Required]
    [RegularExpression(@"\S+", ErrorMessage = "Host must not be empty or whitespace.")]
    public string Host { get; init; } = "127.0.0.1";
    [Range(1, 65535)]
    public int Port { get; init; } = 502;
    [Range(1, 247)]
    public byte UnitId { get; init; } = 1;
    public ModbusFunctionCode FunctionCode { get; init; } = ModbusFunctionCode.ReadHoldingRegisters;
    [Range(0, 65535)]
    public ushort StartAddress { get; init; } = 0;
    [Range(1,125)]
    public ushort RegisterCount { get; init; } = 2;
    [Range(1,86400)]
    public int PollIntervalSeconds { get; init; } = 5;
    [Range(100, 60000)]
    public int ConnectTimeoutMilliseconds { get; init; } = 5000;
    [Range(100, 60000)]
    public int ReadTimeoutMilliseconds { get;  init; } = 5000;
}
