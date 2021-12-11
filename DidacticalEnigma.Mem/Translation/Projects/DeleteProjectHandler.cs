using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class DeleteProjectHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public DeleteProjectHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Delete(
            string? userName,
            string projectName)
        {
            var project = await this.dbContext.Projects
                .Include(project => project.Owner)
                .FirstOrDefaultAsync(project =>
                    (project.Owner.UserName == userName
                     || project.Contributors.Any(contributor => contributor.User.UserName == userName)) &&
                    project.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            if (project.Owner.UserName != userName)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Forbidden,
                    "only the owner of the project can remove a project");
            }

            this.dbContext.Projects.Remove(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}