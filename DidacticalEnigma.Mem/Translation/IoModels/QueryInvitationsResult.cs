using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryInvitationsResult
    {
        public IEnumerable<QueryInvitationSentResult> InvitationsPending { get; set; }
        
        public IEnumerable<QueryInvitationReceivedResult> InvitationsReceived { get; set; }
    }
}