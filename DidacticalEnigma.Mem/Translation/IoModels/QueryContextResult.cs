using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryContextResult
    {
        public Guid Id { get; set; }
        public string? Text { get; set; }
        
        public string CorrelationId { get; set; }
        
        public string? MediaType { get; set; }
        
        public string ProjectName { get; set; }
        
        public bool HasData { get; set; }
    }
}