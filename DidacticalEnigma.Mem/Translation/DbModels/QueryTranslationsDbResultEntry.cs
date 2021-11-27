using System;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class QueryTranslationsDbResultEntry
    {
        public string ParentName { get; set; }
        
        public string Source { get; set; }
        
        public string Target { get; set; }
        
        public string CorrelationId { get; set; }
        
        public Guid? Context { get; set; }
    }
}