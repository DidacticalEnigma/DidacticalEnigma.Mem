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
        
        public async Task<Result<Unit, Unit>> Delete(string? userId, string projectName)
        {
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(project =>
                    (project.OwnerId == userId
                     || project.Contributors.Any(contributor => contributor.UserId == userId)) &&
                    project.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            if (project.OwnerId != userId)
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