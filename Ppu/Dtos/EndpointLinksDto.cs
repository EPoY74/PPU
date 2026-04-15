namespace Ppu.Dtos
{
    public sealed record EndpointLinksDto(
        string Health = "",
        string LastRead = ""
    );
}   