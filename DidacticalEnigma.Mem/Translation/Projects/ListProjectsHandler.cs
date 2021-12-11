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

        public async Task<Result<QueryProjectsResult, object>> Query(
            string? userName)
        {
            var projectInfos = await this.dbContext.Projects
                .Where(project =>
                    project.PublicallyReadable ||
                    (project.Owner.UserName == userName
                     || project.Contributors.Any(contributor => contributor.User.UserName == userName)))
                .Select(project => new
                {
                    ProjectName = project.Name,
                    IsOwner = project.Owner.UserName == userName,
                    CanContribute = project.Owner.UserName == userName ||
                        project.Contributors.Any(contributor => contributor.User.UserName == userName)
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