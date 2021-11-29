using System;
using Microsoft.AspNetCore.Http;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddContextParams
    {
        public Guid Id { get; set; }
        
        public string ProjectName { get; set; }
        
        public string? ContentTypeOverride { get; set; }
        
        public string CorrelationId { get; set; }
        
        public IFormFile Content { get; set; }

        public string? Text { get; set; }
    }
}