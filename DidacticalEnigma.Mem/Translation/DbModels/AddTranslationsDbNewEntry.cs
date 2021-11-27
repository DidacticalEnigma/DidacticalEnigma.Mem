using System;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class AddTranslationsDbNewEntry
    {
        public Guid Id { get; init; }
        
        public string NormalizedSource { get; init; }
        
        public string Source { get; init; }
        
        public string? Target { get; init; }
        
        public string CorrelationId { get; init; }
        
        public Guid? Context { get; init; }
    }
}