namespace Ppu.Dtos
{
    public sealed record RootResponseDto(
        string Application,
        string Version,
        string Status,
        string Description,
        EndpointLinksDto Endpoints
        );
    }
