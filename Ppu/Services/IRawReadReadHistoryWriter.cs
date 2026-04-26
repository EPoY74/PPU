using Ppu.Domain;

namespace Ppu.Services;

public interface IRawReadReadHistoryWriter
{
    Task SaveAsync(RawReadResult result, CancellationToken cancellationToken);    
}