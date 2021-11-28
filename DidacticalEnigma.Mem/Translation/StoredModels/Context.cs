using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Context
    {
        public Guid Id { get; set; }
        
        public Project Project { get; set; }
        
        public int ProjectId { get; set; }
        
        public string CorrelationId { get; set; }

        public string? Text { get; set; }

        public uint? ContentObjectId { get; set; }
        
        public AllowedMediaType? MediaType { get; set; }
    }
}