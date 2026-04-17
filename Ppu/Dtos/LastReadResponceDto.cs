namespace Ppu.Dtos

{
    public sealed record LastReadResponseDto(
        DateTimeOffset TimestampUtc,
        bool IsSuccess,
        string? ErrorMessage,
        int FunctionCode,
        ushort[] Registers, //Array.Empty<ushort>();
        int DurationsMs
    );
}
