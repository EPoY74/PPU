namespace Ppu.Config;

public sealed class HttpServerOptions
{
    public string Scheme { get; init; } = "https";
    public required string Host { get; init; }
    public required int Port { get; init; }
}
