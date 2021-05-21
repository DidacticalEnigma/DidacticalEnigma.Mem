using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryContextResult
    {
        public string? Text { get; set; }
        
        public byte[]? Content { get; set; }
        
        public string? MediaType { get; set; }
    }
}