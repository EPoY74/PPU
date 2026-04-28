using Ppu.Domain;

namespace Ppu.Services;

public interface IRawReadHistoryWriter
{
    Task SaveAsync(RawReadResult result, CancellationToken cancellationToken);    
}