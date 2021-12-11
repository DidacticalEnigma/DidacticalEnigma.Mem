using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class AddProjectHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public AddProjectHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Add(string? userId, string projectName)
        {
            if (userId == null)
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
            project = new Project()
            {
                Name = projectName,
                OwnerId = userId
            };
            this.dbContext.Projects.Add(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}