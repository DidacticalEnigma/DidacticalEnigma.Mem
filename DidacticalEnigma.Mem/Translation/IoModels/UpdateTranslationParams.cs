using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class UpdateTranslationParams
    {
        public string? Source { get; set; }
        
        public string? Target { get; set; }

        public UpdateTranslationNotesParams? TranslationNotes { get; set; }
        
        public IDictionary<string, object>? AssociatedData { get; set; }
        
        public DateTime LastQueryTime { get; set; }
    }
}