using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class ListInvitationsHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public ListInvitationsHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<QueryInvitationsResult, Unit>> Query(
            string? userName)
        {
            var invitationsReceived = (await this.dbContext.Invitations
                .Where(invitation =>
                    invitation.InvitedUser.UserName == userName)
                .Select(invitation => new
                {
                    InvitingUser = invitation.InvitingUser.UserName,
                    ProjectName = invitation.Project.Name
                })
                .ToListAsync())
                .Select(invitation =>
                    new QueryInvitationReceivedResult()
                    {
                        InvitingUser = invitation.InvitingUser,
                        ProjectName = invitation.ProjectName
                    })
                .ToList();
            
            var invitationsSent = (await this.dbContext.Invitations
                    .Where(invitation =>
                        invitation.InvitingUser.UserName == userName)
                    .Select(invitation => new
                    {
                        InvitedUser = invitation.InvitedUser.UserName,
                        ProjectName = invitation.Project.Name
                    })
                    .ToListAsync())
                .Select(invitation =>
                    new QueryInvitationSentResult()
                    {
                        InvitedUser = invitation.InvitedUser,
                        ProjectName = invitation.ProjectName
                    })
                .ToList();
            
            return Result<QueryInvitationsResult, Unit>.Ok(new QueryInvitationsResult()
            {
                InvitationsPending = invitationsSent,
                InvitationsReceived = invitationsReceived
            });
        }
    }
}