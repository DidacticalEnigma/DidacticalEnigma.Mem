using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class UpdateTranslationParams
    {
        public string? Source { get; set; }
        
        public string? Target { get; set; }
        
        public Guid? Context { get; set; }
    }
}