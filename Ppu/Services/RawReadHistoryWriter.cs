using System.Text.Json;

using Ppu.Data;
using Ppu.Data.Entities;
using Ppu.Domain;


namespace Ppu.Services;

public sealed class RawReadHistoryWriter : IRawReadHistoryWriter
{
    private readonly PpuDbContext _dbContext;
    private readonly AppRunContext _appRunContext;
    
    public RawReadHistoryWriter(
        PpuDbContext dbContext,
        AppRunContext appRunContext)
    {
        _dbContext = dbContext;
        _appRunContext = appRunContext;
    }

    public async Task SaveAsync(RawReadResult result, CancellationToken cancellationToken)
    {
        var entry = new RawReadHistoryEntry
        {
            AppRunId = _appRunContext.AppRunId,
            TimestampUtc = result.TimestampUtc,
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            FunctionCode = result.FunctionCode,
            StartAddress = result.StartAddress,
            RegisterCount = result.RegisterCount,
            RegistersJson = JsonSerializer.Serialize(result.Registers),
            DurationMs = result.DurationsMs
        };
        
        _dbContext.RawReadHistory.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}