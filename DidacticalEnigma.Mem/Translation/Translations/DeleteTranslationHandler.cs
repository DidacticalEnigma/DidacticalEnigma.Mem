using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Translations
{
    public class DeleteTranslationHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public DeleteTranslationHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Delete(
            string? userId,
            string projectName,
            string correlationId)
        {
            var translation = await this.dbContext.TranslationPairs
                .FirstOrDefaultAsync(t =>
                    (t.Parent.OwnerId == userId
                        || t.Parent.Contributors.Any(contributor => contributor.UserId == userId)) &&
                    t.Parent.Name == projectName &&
                    t.CorrelationId == correlationId);
            
            if (translation == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "translation not found");
            }

            this.dbContext.TranslationPairs.Remove(translation);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}