using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class NotesCollection
    {
        public List<GlossNote> GlossNotes { get; set; }
        
        public List<NormalNote> NormalNote { get; set; }
    }
}