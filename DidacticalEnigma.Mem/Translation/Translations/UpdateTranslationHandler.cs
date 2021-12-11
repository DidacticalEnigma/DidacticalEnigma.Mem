using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Translations
{
    public class UpdateTranslationHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public UpdateTranslationHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, QueryTranslationResult>> Update(
            string? userName,
            string projectName,
            string correlationId,
            UpdateTranslationParams uploadParams)
        {
            var translation = await this.dbContext.TranslationPairs
                .Include(t => t.Category)
                .Include(t => t.Parent)
                .FirstOrDefaultAsync(t =>
                    (t.Parent.Owner.UserName == userName 
                        || t.Parent.Contributors.Any(contributor => contributor.User.UserName == userName)) &&
                    t.Parent.Name == projectName &&
                    t.CorrelationId == correlationId);
            
            if (translation == null)
            {
                return Result<Unit, QueryTranslationResult>.Failure(
                    HttpStatusCode.NotFound,
                    "translation not found");
            }

            var currentTime = this.currentTimeProvider.GetCurrentTime();

            if (uploadParams.LastQueryTime < translation.ModificationTime)
            {
                return Result<Unit, QueryTranslationResult>.Failure(
                    HttpStatusCode.Conflict,
                    "translation was updated from the time it last requested",
                    new QueryTranslationResult(
                        projectName: translation.Parent.Name,
                        source: translation.Source,
                        target: translation.Target,
                        highlighterSequence: null,
                        category: translation.Category?.Name,
                        categoryId: translation.CategoryId,
                        correlationId: translation.CorrelationId,
                        translationNotes: Mapper.Map(translation.Notes),
                        associatedData: translation.AssociatedData != null
                            ? JsonSerializer.Deserialize<IDictionary<string, object>>(translation.AssociatedData)
                            : null));
            }
            
            if (uploadParams.Source != null)
            {
                translation.Source = uploadParams.Source;
                translation.SearchVector = await this.dbContext.ToTsVector(uploadParams.Source);
                translation.ModificationTime = currentTime;
            }

            if (uploadParams.Target != null)
            {
                translation.Target = uploadParams.Target;
                translation.ModificationTime = currentTime;
            }
            
            if (uploadParams.CategoryId != null)
            {
                translation.CategoryId = uploadParams.CategoryId;
                translation.ModificationTime = currentTime;
            }
            
            if (uploadParams.TranslationNotes != null)
            {
                translation.Notes = Mapper.Map(uploadParams.TranslationNotes);
                translation.ModificationTime = currentTime;
            }
            
            if (uploadParams.AssociatedData != null)
            {
                translation.AssociatedData = JsonSerializer.Serialize(uploadParams.AssociatedData);
                translation.ModificationTime = currentTime;
            }

            await this.dbContext.SaveChangesAsync();
            return Result<Unit, QueryTranslationResult>.Ok(Unit.Value);
        }
    }
}