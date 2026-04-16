using Ppu.Config;
using Ppu.Dtos;
using Ppu.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PlcReaderOptions>(
    builder.Configuration.GetSection("PlcReader"));

builder.Services.AddSingleton<LastReadStore>();
builder.Services.AddSingleton<IPlcReader, PlcReaderService>();
builder.Services.AddHostedService<PollingWorker>();


var app = builder.Build();
var rootResponse = new RootResponseDto(
    Application: "PPU",
    Version: "0.1.1",
    Status: "Running",
    Description: "Simple modbus TCP PLC polling utility with HTTP API.",
    Endpoints: new EndpointLinksDto(
        Health: "/health",
        LastRead: "/last-read"
    )
);

app.MapGet("/", () => Results.Ok(rootResponse));

app.MapGet("/health", () =>
{
    var dto = new HealthResponseDto(
        "PPU", 
        "ok", 
        DateTimeOffset.UtcNow);
    return Results.Ok(dto);
});
app.MapGet("/last-read", (LastReadStore store) =>
{
    var result = store.Get();

    return result is null
        ? Results.NotFound(new { message = "No read yet." })
        : Results.Ok(result);
});


app.Run();

