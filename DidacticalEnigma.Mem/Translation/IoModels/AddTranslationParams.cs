using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationParams
    {
        public string Source { get; set; }

        public string? Target { get; set; }
        
        public string CorrelationId { get; set; }
        
        public Guid? Context { get; set; }
    }
}