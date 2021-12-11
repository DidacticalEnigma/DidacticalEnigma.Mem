using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class AddProjectHandler
    {
        private readonly MemContext dbContext;
        private readonly UserManager<User> userManager;

        public AddProjectHandler(
            MemContext dbContext,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        
        public async Task<Result<Unit, Unit>> Add(
            string? userName,
            string projectName,
            bool publicallyReadable = true)
        {
            if (userName == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Forbidden,
                    "unregistered users can't create projects");
            }
            
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(
                p => p.Name == projectName);
            if (project != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Conflict,
                    "project with a given name already exists");
            }

            var user = await this.userManager.FindByNameAsync(userName);
            project = new Project()
            {
                Name = projectName,
                Owner = user,
                OwnerId = user.Id,
                PublicallyReadable = publicallyReadable
            };
            this.dbContext.Projects.Add(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}