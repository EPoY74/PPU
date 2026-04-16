namespace Ppu.Dtos
{
    public sealed record HealthResponseDto(
        string Application,
        string Status,
        DateTimeOffset Utc
    );
}
