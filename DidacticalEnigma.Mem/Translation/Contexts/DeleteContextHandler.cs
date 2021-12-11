using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Contexts
{
    public class DeleteContextHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public DeleteContextHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Delete(
            string? userName,
            Guid id)
        {
            var context = await this.dbContext.Contexts
                .FirstOrDefaultAsync(context =>
                    (context.Project.Owner.UserName == userName
                     || context.Project.Contributors.Any(contributor => contributor.User.UserName == userName)) &&
                    context.Id == id);

            if (context == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context not found");
            }

            this.dbContext.Contexts.Remove(context);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}