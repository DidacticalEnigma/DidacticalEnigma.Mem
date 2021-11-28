using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class UpdateTranslationNotesParams
    {
        public IReadOnlyList<IoNormalNote> Normal { get; set; }
        
        public IReadOnlyList<IoGlossNote> Gloss { get; set; }
    }
}