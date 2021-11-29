using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationsParams
    {
        public bool AllowPartialAdd { get; set; }
        
        public IReadOnlyCollection<AddTranslationParams> Translations { get; set; }
    }
}