using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Translations
{
    public class QueryTranslationsHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public QueryTranslationsHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<QueryTranslationsResult, Unit>> Query(
            string? userName,
            string? projectName,
            string? correlationIdStart,
            string? queryText,
            string? category,
            string? paginationToken = null,
            int? limit = null)
        {
            if (projectName == null && correlationIdStart == null && queryText == null && category == null)
            {
                return Result<QueryTranslationsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "one of: projectName, correlationId, queryText, category must be provided");
            }
            var resultLimit = Math.Min(limit ?? 50, 250);
            var translations = this.dbContext.TranslationPairs
                .OrderBy(translation => translation.CorrelationId)
                .AsQueryable();

            if (userName == null)
            {
                translations = translations.Where(translation => translation.Parent.PublicallyReadable);
            }
            else
            {
                translations = translations.Where(translation =>
                    translation.Parent.PublicallyReadable
                    || translation.Parent.Owner.UserName == userName
                    || translation.Parent.Contributors.Any(contributor => contributor.User.UserName == userName));
            }
            
            if(projectName != null)
                translations = translations.Where(translationPair => translationPair.Parent.Name == projectName);
            if(correlationIdStart != null)
                translations = translations.Where(translationPair => translationPair.CorrelationId.StartsWith(correlationIdStart));
            if (queryText != null)
            {
                var normalized = analyzer.Normalize(queryText);
                translations = translations.Where(translationPair =>
                    translationPair.SearchVector.Matches(
                        EF.Functions.PhraseToTsQuery("simple", normalized)));
            }
            if (category != null)
                translations = translations.Where(translationPair => translationPair.Category != null && translationPair.Category.Name == category);

            if (paginationToken != null)
            {
                var decodedPaginationToken = Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken));

                translations = translations
                    .Where(translation => string.Compare(decodedPaginationToken, translation.CorrelationId) < 0);
            }

            var results = (await translations
                    .Take(resultLimit)
                    .Select(translationPair => new
                    {
                        ParentName = translationPair.Parent.Name,
                        Source = translationPair.Source,
                        Target = translationPair.Target,
                        CategoryName = translationPair.Category != null
                            ? translationPair.Category.Name
                            : null,
                        CategoryId = translationPair.CategoryId,
                        CorrelationId = translationPair.CorrelationId,
                        Notes = translationPair.Notes,
                        AssociatedData = translationPair.AssociatedData
                    })
                    .ToListAsync())
                .Select(selection =>
                {
                    var (source, highlighter) = queryText != null
                        ? analyzer.Highlight(selection.Source, queryText)
                        : (selection.Source, null);
                    return new QueryTranslationResult(
                        projectName: selection.ParentName,
                        source: source,
                        target: selection.Target,
                        highlighterSequence: highlighter,
                        category: selection.CategoryName,
                        categoryId: selection.CategoryId,
                        correlationId: selection.CorrelationId,
                        translationNotes: Mapper.Map(selection.Notes),
                        associatedData: selection.AssociatedData != null
                            ? JsonSerializer.Deserialize<IDictionary<string, object>>(selection.AssociatedData)
                            : null);
                })
                .ToList();

            string? newPaginationToken = null;
            
            if (results.Count < limit)
            {
                newPaginationToken = null;
            }
            else
            {
                newPaginationToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(results.Last().CorrelationId));
            }

            var queryTime = this.currentTimeProvider.GetCurrentTime();
            
            return Result<QueryTranslationsResult, Unit>.Ok(
                new QueryTranslationsResult(
                    results,
                    newPaginationToken,
                    queryTime));
        }
    }
}