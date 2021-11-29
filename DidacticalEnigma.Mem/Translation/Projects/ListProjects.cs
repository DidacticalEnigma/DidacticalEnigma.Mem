using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class ListProjects
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public ListProjects(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }

        public async Task<Result<QueryProjectsResult, object>> Query()
        {
            var projectNames = await this.dbContext.Projects
                .Select(project => project.Name)
                .ToListAsync();
            
            return Result<QueryProjectsResult, object>.Ok(new QueryProjectsResult()
            {
                Projects = projectNames
                    .Select(projectName => new QueryProjectResult()
                    {
                        Name = projectName
                    })
                    .ToList()
            });
        }
    }
}