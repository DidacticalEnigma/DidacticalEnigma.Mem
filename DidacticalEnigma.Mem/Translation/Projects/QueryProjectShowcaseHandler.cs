using System.Linq;
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
    public class QueryProjectShowcaseHandler
    {
        private readonly MemContext dbContext;
        private readonly ICurrentTimeProvider currentTimeProvider;
        private readonly UserManager<User> userManager;

        public QueryProjectShowcaseHandler(
            MemContext dbContext,
            ICurrentTimeProvider currentTimeProvider,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.currentTimeProvider = currentTimeProvider;
            this.userManager = userManager;
        }

        public async Task<Result<QueryProjectShowcaseResult, Unit>> Query(
            string? userName,
            string projectName)
        {
            var projectShowcase = await this.dbContext.Projects
                .Where(p =>
                    p.Name == projectName &&
                    (p.PublicallyReadable ||
                    p.Owner.UserName == userName ||
                    p.Contributors.Any(contributor => contributor.User.UserName == userName)))
                .Select(p => new
                {
                    ProjectName = p.Name,
                    Owner = 
                        p.Owner.UserName == userName ||
                        p.Contributors.Any(contributor => contributor.User.UserName == userName)
                        ? p.Owner.UserName
                        : null,
                    Contributors =
                        p.Owner.UserName == userName ||
                        p.Contributors.Any(contributor => contributor.User.UserName == userName)
                        ? p.Contributors.Select(contributor => contributor.User.UserName)
                        : null,
                    TotalUntranslatedLines =
                        p.Translations.Count(t => t.Target == null),
                    RecentAddedLines =
                        p.Translations
                            .OrderByDescending(t => t.ModificationTime)
                            .Where(t => t.Target != null)
                            .Take(10)
                            .Select(t => new {t.Source, t.Target, t.CorrelationId}),
                    UntranslatedLines =
                        p.Translations
                        .OrderByDescending(t => t.ModificationTime)
                        .Where(t => t.Target == null)
                        .Take(10)
                        .Select(t => new {t.Source, t.CorrelationId})

                })
                .FirstOrDefaultAsync();

            if (projectShowcase == null)
            {
                return Result<QueryProjectShowcaseResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }
            
            return Result<QueryProjectShowcaseResult, Unit>.Ok(new QueryProjectShowcaseResult()
            {
                ProjectName = projectShowcase.ProjectName,
                Owner = projectShowcase.Owner,
                Contributors = projectShowcase.Contributors?.ToList(),
                TotalUntranslatedLines = projectShowcase.TotalUntranslatedLines,
                RecentAddedLines = projectShowcase.RecentAddedLines
                    .Select(line => new QueryProjectShowcaseTranslationResult()
                    {
                        Source = line.Source,
                        Target = line.Target,
                        CorrelationId = line.CorrelationId
                    })
                    .ToList(),
                UntranslatedLines = projectShowcase.UntranslatedLines
                    .Select(line => new QueryProjectShowcaseTranslationResult()
                    {
                        Source = line.Source,
                        Target = null,
                        CorrelationId = line.CorrelationId
                    })
                    .ToList()
                
            });
        }
    }
}