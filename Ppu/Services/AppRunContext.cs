namespace Ppu.Services;

public class AppRunContext
{
    public Guid AppRunId { get; } =  Guid.NewGuid();
}