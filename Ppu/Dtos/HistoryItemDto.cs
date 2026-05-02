namespace Ppu.Dtos;

public sealed record HistoryItemDto(
    long Id,
    Guid AppRunId,
    DateTimeOffset TimestampUtc,
    bool IsSuccess,
    string? ErrorMessage,
    ushort FunctionCode,
    ushort StartAddress,
    ushort RegisterCount,
    string RegistersJson,
    int DurationMs);
