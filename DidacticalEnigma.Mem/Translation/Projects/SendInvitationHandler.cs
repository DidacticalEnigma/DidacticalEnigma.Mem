using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class SendInvitationHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public SendInvitationHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<Unit, Unit>> Send(
            string? userId,
            string projectName,
            string invitedUserName)
        {
            if (userId == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Forbidden,
                    "unregistered users can't send invitations");
            }
            
            var projectInfo = await this.dbContext.Projects
                .Where(project =>
                    project.Name == projectName)
                .Select(project => new
                {
                    ProjectId = project.Id,
                    IsOwner = project.OwnerId == userId,
                    IsVisible =
                        project.PublicallyReadable ||
                        project.OwnerId == userId ||
                        project.Contributors.Any(contributor => contributor.UserId == userId)
                })
                .FirstOrDefaultAsync();

            if (projectInfo == null || !projectInfo.IsVisible)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            if (!projectInfo.IsOwner)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Forbidden,
                    "only the owner can invite contributors");
            }

            this.dbContext.Invitations.Add(new ContributorInvitation()
            {
                InvitingUserId = userId,
                ProjectId = projectInfo.ProjectId,
                InvitedUser = await this.userManager.FindByNameAsync(invitedUserName)
            });

            await this.dbContext.SaveChangesAsync();
            
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}