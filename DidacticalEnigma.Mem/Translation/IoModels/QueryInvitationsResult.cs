using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryInvitationsResult
    {
        public IEnumerable<QueryInvitationResult> Invitations { get; set; }
    }
}