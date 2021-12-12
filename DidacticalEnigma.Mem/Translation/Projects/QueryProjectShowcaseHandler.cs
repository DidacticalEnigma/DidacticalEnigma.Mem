using System.Collections.Generic;
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
            var projectId = await this.dbContext.Projects
                .Where(p =>
                    p.Name == projectName &&
                    (p.PublicallyReadable ||
                    p.Owner.UserName == userName ||
                    p.Contributors.Any(contributor => contributor.User.UserName == userName)))
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
            if (projectId == null)
            {
                return Result<QueryProjectShowcaseResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            var isContributor = await this.dbContext.Projects
                .AnyAsync(p =>
                    p.Name == projectName &&
                    p.Owner.UserName == userName ||
                    p.Contributors.Any(contributor => contributor.User.UserName == userName));

            string? owner = null;
            IReadOnlyCollection<string>? contributors = null;
            if (isContributor)
            {
                owner = await this.dbContext.Projects
                    .Where(project => project.Id == projectId)
                    .Select(project => project.Owner.UserName).FirstOrDefaultAsync();

                contributors = await this.dbContext.Users
                    .Where(user => user.ContributedProjects.Any(p => p.ProjectId == projectId))
                    .Select(user => user.UserName)
                    .ToListAsync();
            }

            int totalUntranslatedLines = await this.dbContext.TranslationPairs
                .CountAsync(translation => translation.ParentId == projectId && translation.Target == null);

            var recentLines = await this.dbContext.TranslationPairs
                .Where(t => t.ParentId == projectId)
                .OrderByDescending(t => t.ModificationTime)
                .Where(t => t.Target != null)
                .Take(10)
                .Select(t => new { t.Source, t.Target, t.CorrelationId })
                .ToListAsync();
            
            var untranslatedLines = await this.dbContext.TranslationPairs
                .Where(t => t.ParentId == projectId)
                .OrderByDescending(t => t.ModificationTime)
                .Where(t => t.Target == null)
                .Take(10)
                .Select(t => new {t.Source, t.CorrelationId})
                .ToListAsync();

            return Result<QueryProjectShowcaseResult, Unit>.Ok(new QueryProjectShowcaseResult()
            {
                ProjectName = projectName,
                Owner = owner,
                Contributors = contributors,
                TotalUntranslatedLines = totalUntranslatedLines,
                RecentAddedLines = recentLines
                    .Select(line => new QueryProjectShowcaseTranslationResult()
                    {
                        Source = line.Source,
                        Target = line.Target,
                        CorrelationId = line.CorrelationId
                    })
                    .ToList(),
                UntranslatedLines = untranslatedLines
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