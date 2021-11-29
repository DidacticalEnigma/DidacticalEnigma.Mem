using System;

namespace DidacticalEnigma.Mem.Services
{
    class CurrentTimeProvider : ICurrentTimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}