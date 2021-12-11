using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Categories
{
    public class AddCategoriesHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public AddCategoriesHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Add(
            string? userName,
            string projectName,
            AddCategoriesParams categoriesParams)
        {
            var projectId = await this.dbContext.Projects
                .Where(project =>
                    (project.Owner.UserName == userName
                     || project.Contributors.Any(contributor => contributor.User.UserName == userName)) &&
                    project.Name == projectName)
                .Select(project => (int?)project.Id)
                .FirstOrDefaultAsync();

            if (projectId == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            foreach (var addCategoryParams in categoriesParams.Categories)
            {
                this.dbContext.Categories.Add(new Category()
                {
                    Id = addCategoryParams.Id,
                    Name = addCategoryParams.Name,
                    ParentId = projectId.Value
                });
            }

            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}