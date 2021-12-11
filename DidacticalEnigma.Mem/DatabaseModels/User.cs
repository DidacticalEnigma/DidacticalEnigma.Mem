using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class User : IdentityUser
    {
        public IReadOnlyCollection<ContributorMembership> ContributedProjects { get; set; }
        
        public IReadOnlyCollection<Project> OwnedProjects { get; set; }
        
        public IReadOnlyCollection<ContributorInvitation> InvitationsReceived { get; set; }
        
        public IReadOnlyCollection<ContributorInvitation> InvitationsSent { get; set; }
    }
}