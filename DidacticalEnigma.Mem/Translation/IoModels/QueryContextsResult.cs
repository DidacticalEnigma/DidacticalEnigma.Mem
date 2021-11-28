using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryContextsResult
    {
        public IReadOnlyCollection<QueryContextResult> Contexts { get; set; }
    }
}