using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddContext
    {
        public Guid Id { get; set; }
        
        public byte[]? Content { get; set; }
        
        public string? MediaType { get; set; }
        
        public string? Text { get; set; }
    }
}