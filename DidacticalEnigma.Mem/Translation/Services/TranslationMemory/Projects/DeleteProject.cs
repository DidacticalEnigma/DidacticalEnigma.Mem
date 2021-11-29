using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Services.TranslationMemory.Projects
{
    public class DeleteProject
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public DeleteProject(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Delete(string projectName)
        {
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(p => p.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            this.dbContext.Projects.Remove(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}