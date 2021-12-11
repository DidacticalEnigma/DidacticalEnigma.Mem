using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Categories
{
    public class QueryCategoriesHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public QueryCategoriesHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<QueryCategoriesResult, Unit>> Query(
            string? userId,
            string projectName)
        {
            if (!this.dbContext.Projects.Any(project =>
                    (project.PublicallyReadable ||
                     project.OwnerId == userId ||
                     project.Contributors.Any(contributor => contributor.UserId == userId)) &&
                    project.Name == projectName))
            {
                return Result<QueryCategoriesResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }
            
            var categories = (await this.dbContext.Projects
                    .Where(project => project.Name == projectName)
                    .SelectMany(project => project.Categories)
                    .Select(category => new
                    {
                        Id = category.Id,
                        Name = category.Name
                    })
                    .ToListAsync())
                .Select(categoryData => new QueryCategoryResult()
                {
                    Id = categoryData.Id,
                    Name = categoryData.Name
                })
                .ToList();
            
            return Result<QueryCategoriesResult, Unit>.Ok(new QueryCategoriesResult()
            {
                Categories = categories 
            });
        }
    }
}