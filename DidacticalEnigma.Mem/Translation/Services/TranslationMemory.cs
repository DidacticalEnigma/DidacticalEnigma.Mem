using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.EntityFrameworkCore;
using Utility.Utils;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public class TranslationMemory : ITranslationMemory
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;

        public TranslationMemory(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
        }

        public async Task<QueryResult> Query(string? projectName, string? correlationIdStart, string? queryText, int limit = 50)
        {
            if (projectName == null && correlationIdStart == null && queryText == null)
            {
                throw new InvalidOperationException();
            }
            limit = Math.Min(limit, 250);
            var translations = this.dbContext.TranslationPairs.AsQueryable();
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

            var results = (await translations
                    .Take(limit)
                    .Select(translationPair => new
                    {
                        ParentName = translationPair.Parent.Name,
                        Source = translationPair.Source,
                        Target = translationPair.Target,
                        CorrelationId = translationPair.CorrelationId
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
                        correlationId: selection.CorrelationId);
                });
            return new QueryResult(results);
        }

        public async Task AddProject(string projectName)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project != null)
            {
                throw new InvalidOperationException();
            }
            project = new Project()
            {
                Name = projectName
            };
            this.dbContext.Projects.Add(project);
        }

        public async Task AddTranslations(string projectName, IEnumerable<AddTranslation> translations)
        {
            var project = await this.dbContext.Projects.FirstAsync(p => p.Name == projectName);
            foreach (var translation in translations)
            {
                var model = new StoredModels.Translation()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = translation.CorrelationId,
                    Parent = project,
                    Target = translation.Target,
                    Source = translation.Source
                };
                model.SearchVector = await this.dbContext.ToTsVector(analyzer.Normalize(translation.Source));
                this.dbContext.TranslationPairs.Add(model);
            }
        }

        public async Task AddContext(Guid id, byte[]? content, string? mediaType, string? text)
        {
            var context = await this.dbContext.Contexts.FindAsync(id);
            if (context != null)
            {
                throw new InvalidOperationException();
            }

            if (text == null && mediaType == null && content == null)
            {
                throw new InvalidOperationException();
            }

            if (content != null && mediaType == null)
            {
                throw new InvalidOperationException();
            }
            
            if (content == null && mediaType != null)
            {
                throw new InvalidOperationException();
            }

            AllowedMediaType? mediaTypeModel = null;
            if (mediaType != null)
            {
                mediaTypeModel = await this.dbContext.MediaTypes.FirstOrDefaultAsync(m => m.MediaType == mediaType);
                if (mediaTypeModel == null)
                {
                    throw new InvalidOperationException();
                }
            }

            this.dbContext.Contexts.Add(new Context()
            {
                Id = id,
                Content = content,
                MediaType = mediaTypeModel,
                Text = text
            });
        }

        public async Task SaveChanges()
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}