using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddContextParams
    {
        public Guid Id { get; set; }
        
        public byte[]? Content { get; set; }
        
        public string? MediaType { get; set; }
        
        public string? Text { get; set; }
    }
}