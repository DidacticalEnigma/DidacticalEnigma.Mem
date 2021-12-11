using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Projects
{
    public class ListProjectsHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public ListProjectsHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }

        public async Task<Result<QueryProjectsResult, object>> Query(string? userId)
        {
            var projectInfos = await this.dbContext.Projects
                .Where(project =>
                    project.PublicallyReadable ||
                    (project.OwnerId == userId
                     || project.Contributors.Any(contributor => contributor.UserId == userId)))
                .Select(project => new
                {
                    ProjectName = project.Name,
                    IsOwner = project.OwnerId == userId,
                    CanContribute = project.OwnerId == userId ||
                        project.Contributors.Any(contributor => contributor.UserId == userId)
                })
                .ToListAsync();
            
            return Result<QueryProjectsResult, object>.Ok(new QueryProjectsResult()
            {
                Projects = projectInfos
                    .Select(projectInfo => new QueryProjectResult()
                    {
                        Name = projectInfo.ProjectName,
                        IsOwner = projectInfo.IsOwner,
                        CanContribute = projectInfo.CanContribute
                    })
                    .ToList()
            });
        }
    }
}