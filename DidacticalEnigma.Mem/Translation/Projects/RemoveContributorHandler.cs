using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class RemoveContributorHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public RemoveContributorHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<Result<Unit, Unit>> Remove(
            string? userName,
            string projectName,
            string contributorName)
        {
            var membership = await this.dbContext.Memberships
                .FirstOrDefaultAsync(membership =>
                    membership.Project.Owner.UserName == userName &&
                    membership.User.UserName == contributorName &&
                    membership.Project.Name == projectName);

            if (membership == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "contributor not found for this project");
            }
            
            this.dbContext.Memberships.Remove(membership);

            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}