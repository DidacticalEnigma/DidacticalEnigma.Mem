using System.Collections.Generic;
using DidacticalEnigma.Mem.Translation.StoredModels;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslations
    {
        public IReadOnlyCollection<AddTranslation> Translations { get; set; }
    }
}