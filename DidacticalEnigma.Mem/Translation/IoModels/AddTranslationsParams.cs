using System.Collections.Generic;
using DidacticalEnigma.Mem.Translation.StoredModels;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationsParams
    {
        public IReadOnlyCollection<AddTranslationParams> Translations { get; set; }
    }
}