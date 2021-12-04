using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationsParams
    {
        /// <summary>
        /// Controls whether to add translations if only a part of them could be inserted (because the correlation ids of others are already added for the project)
        /// </summary>
        public bool AllowPartialAdd { get; set; }
        
        public IReadOnlyCollection<AddTranslationParams> Translations { get; set; }
    }
}