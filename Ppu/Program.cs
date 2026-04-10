using Ppu.Config;
using Ppu.Domain;
using Ppu.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PlcReaderOptions>(
    builder.Configuration.GetSection("PlcReader"));

builder.Services.AddSingleton<LastReadStore>();
builder.Services.AddSingleton<IPlcReader, PlcReaderService>();
builder.Services.AddHostedService<PollingWorker>();


var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    application = "PPU",
    status = "ok",
    utc = DateTimeOffset.UtcNow
}));

app.MapGet("/last-read", (LastReadStore store) =>
{
    var result = store.Get();

    return result is null
        ? Results.NotFound(new { message = "No read yet." })
        : Results.Ok(result);
});


app.Run();

