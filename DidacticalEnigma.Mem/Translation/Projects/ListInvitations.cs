using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class ListInvitations
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public ListInvitations(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<QueryInvitationsResult, Unit>> Query(
            string? userId)
        {
            var invitations = await this.dbContext.Invitations
                .Where(invitation =>
                    invitation.InvitedUserId == userId)
                .Select(invitation => new
                {
                    InvitingUser = invitation.InvitingUser.UserName,
                    ProjectName = invitation.Project.Name
                })
                .ToListAsync();
            
            return Result<QueryInvitationsResult, Unit>.Ok(new QueryInvitationsResult()
            {
                Invitations = invitations
                    .Select(invitation =>
                        new QueryInvitationResult()
                        {
                            InvitingUser = invitation.InvitingUser,
                            ProjectName = invitation.ProjectName
                        })
                    .ToList()
            });
        }
    }
}