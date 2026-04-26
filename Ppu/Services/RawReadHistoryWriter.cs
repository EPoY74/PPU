using System.Text.Json;

using Ppu.Data;
using Ppu.Data.Entities;
using Ppu.Domain;


namespace Ppu.Services;

public sealed class RawReadHistoryWriter : IRawReadReadHistoryWriter
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
            TimeStampUtc = result.TimestampUtc,
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            FunctionCode = result.FunctionCode,
            StartAddress = 0, // временно
            RegisterCouunt = (ushort)(result.Registers?.Length ?? 0),
            RegistersJson = JsonSerializer.Serialize(result.Registers),
            Duration = result.DurationsMs
        };
        
        _dbContext.RawReadHistory.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}