using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryProjectShowcaseResult
    {
        public string ProjectName { get; set; }
        
        public string? Owner { get; set; }
        
        public int TotalUntranslatedLines { get; set; }
        
        public IReadOnlyCollection<string>? Contributors { get; set; }
        
        public IReadOnlyCollection<QueryProjectShowcaseTranslationResult> RecentAddedLines { get; set; }
        
        public IReadOnlyCollection<QueryProjectShowcaseTranslationResult> UntranslatedLines { get; set; }
    }
}