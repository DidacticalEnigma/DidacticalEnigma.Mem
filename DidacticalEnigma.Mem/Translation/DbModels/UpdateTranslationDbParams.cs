using System;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class UpdateTranslationDbParams
    {
        public string InputProjectName { get; init; }
        
        public string InputCorrelationId { get; init; }
        
        public DateTime CurrentTime { get; init; }
        
        public string? InputNormalizedSource { get; init; }
        
        public string? InputSource { get; init; }
        
        public string? InputTarget { get; init; }
        
        public Guid? InputContextId { get; init; }
    }
}