using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Services.TranslationMemory.Projects
{
    public class AddProject
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public AddProject(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Add(string projectName)
        {
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
                Name = projectName
            };
            this.dbContext.Projects.Add(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}