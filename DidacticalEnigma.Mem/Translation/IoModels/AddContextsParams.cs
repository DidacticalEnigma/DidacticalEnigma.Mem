using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddContextsParams
    {
        public IReadOnlyCollection<AddContextParams> Contexts { get; set; }
    }
}