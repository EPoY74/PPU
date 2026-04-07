using Ppu.Config;
using Ppu.Domain;

namespace Ppu.Services;

public interface IPlcReader
{
    Task<RawReadResult> RawReadAsync(CancellationToken cancellationToken);
}