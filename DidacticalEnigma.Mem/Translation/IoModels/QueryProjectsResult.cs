using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryProjectsResult
    {
        public IReadOnlyCollection<QueryProjectResult> Projects { get; set; }
    }
}