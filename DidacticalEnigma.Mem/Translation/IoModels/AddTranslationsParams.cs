using System.Collections.Generic;
using DidacticalEnigma.Mem.Translation.StoredModels;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationsParams
    {
        public bool AllowPartialAdd { get; set; }
        
        public IReadOnlyCollection<AddTranslationParams> Translations { get; set; }
    }
}