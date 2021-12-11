using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryInvitationsResult
    {
        public IReadOnlyCollection<QueryInvitationSentResult> InvitationsPending { get; set; }
        
        public IReadOnlyCollection<QueryInvitationReceivedResult> InvitationsReceived { get; set; }
    }
}