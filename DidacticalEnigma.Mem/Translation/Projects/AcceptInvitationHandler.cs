using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class AcceptInvitationHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public AcceptInvitationHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<Unit, Unit>> Accept(
            string? userName,
            string projectName,
            string invitingUserName)
        {
            var invitation = await this.dbContext.Invitations
                .FirstOrDefaultAsync(invitation =>
                    invitation.Project.Name == projectName &&
                    invitation.InvitingUser.UserName == invitingUserName &&
                    invitation.InvitedUser.UserName == userName);

            if (invitation == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "invitation not found");
            }

            this.dbContext.Invitations.Remove(invitation);
            this.dbContext.Memberships.Add(invitation.CreateMembership());

            await this.dbContext.SaveChangesAsync();
            
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}