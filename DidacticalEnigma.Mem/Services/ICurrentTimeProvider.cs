using System;

namespace DidacticalEnigma.Mem.Services
{
    public interface ICurrentTimeProvider
    {
        DateTime GetCurrentTime();
    }
}