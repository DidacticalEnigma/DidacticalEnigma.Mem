using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Categories
{
    public class QueryCategories
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public QueryCategories(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<QueryCategoriesResult, Unit>> Query(string projectName)
        {
            if (!this.dbContext.Projects.Any(project => project.Name == projectName))
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