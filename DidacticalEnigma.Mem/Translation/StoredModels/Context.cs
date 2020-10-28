using System;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Context
    {
        public Guid Id { get; set; }
        
        public string? Text { get; set; }
        
        public byte[]? Content { get; set; }
        
        public AllowedMediaType? MediaType { get; set; }
    }
}