using System;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public interface ICurrentTimeProvider
    {
        DateTime GetCurrentTime();
    }
}