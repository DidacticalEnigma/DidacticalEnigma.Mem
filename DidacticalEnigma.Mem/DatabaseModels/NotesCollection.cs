using System.Collections.Generic;

namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class NotesCollection
    {
        public List<GlossNote> GlossNotes { get; set; }
        
        public List<NormalNote> NormalNote { get; set; }
    }
}