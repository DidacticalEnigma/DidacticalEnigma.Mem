using System;

namespace DidacticalEnigma.Mem.Translation.Services
{
    class CurrentTimeProvider : ICurrentTimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}