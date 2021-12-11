using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class CancelInvitationHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public CancelInvitationHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<Unit, Unit>> Reject(
            string? userName,
            string projectName,
            string invitedUserName)
        {
            var invitation = await this.dbContext.Invitations
                .FirstOrDefaultAsync(invitation =>
                    invitation.Project.Name == projectName &&
                    invitation.InvitingUser.UserName == userName &&
                    invitation.InvitedUser.UserName == invitedUserName);

            if (invitation == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "invitation not found");
            }
            
            this.dbContext.Invitations.Remove(invitation);

            await this.dbContext.SaveChangesAsync();
            
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}