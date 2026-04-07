using Ppu.Domain;

namespace Ppu.Services;

public sealed class LastReadStore 
{
    private RawReadResult? _lastRead;
    public RawReadResult? Get() =>  _lastRead;
    public void Set (RawReadResult readResult)
    {
        _lastRead = readResult;

    }


}