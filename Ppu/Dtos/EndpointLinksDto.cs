namespace Ppu.Dtos
{
    public sealed record EndpointLinksDto(
        string HealthEndpoint = "",
        string LastReadEndpoint = "",
        string OpenApiEndpoint = ""
    );
}   