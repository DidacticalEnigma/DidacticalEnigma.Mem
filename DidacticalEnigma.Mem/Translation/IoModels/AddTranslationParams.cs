using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationParams
    {
        public string Source { get; set; }

        public string? Target { get; set; }
        
        public string CorrelationId { get; set; }
        
        public Guid? CategoryId { get; set; }

        public AddTranslationNotesParams? TranslationNotes { get; set; }
        
        public IDictionary<string, object>? AssociatedData { get; set; }
    }
}