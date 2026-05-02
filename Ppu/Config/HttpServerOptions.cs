namespace Ppu.Config;

public sealed class HttpServerOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
}
