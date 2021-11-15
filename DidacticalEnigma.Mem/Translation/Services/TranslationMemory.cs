using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly ICurrentTimeProvider currentTimeProvider;

        public TranslationMemory(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }

        public async Task<Result<QueryResult>> Query(
            string? projectName,
            string? correlationIdStart,
            string? queryText,
            int limit = 50)
        {
            if (projectName == null && correlationIdStart == null && queryText == null)
            {
                return Result<QueryResult>.Failure(
                    HttpStatusCode.BadRequest,
                    "one of: projectName, correlationId, queryText must be provided");
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
                        CorrelationId = translationPair.CorrelationId,
                        Context = translationPair.Context != null ? translationPair.Context.Id : (Guid?) null
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
                        correlationId: selection.CorrelationId,
                        context: selection.Context);
                });
            return Result<QueryResult>.Ok(new QueryResult(results));
        }

        public async Task<Result<QueryContextResult>> GetContext(Guid id)
        {
            var contextData = await this.dbContext.Contexts
                .Where(context => context.Id == id)
                .Select(context => new {
                    Content = context.Content,
                    MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                    Text = context.Text
                })
                .FirstOrDefaultAsync();

            if (contextData == null)
            {
                return Result<QueryContextResult>.Failure(
                    HttpStatusCode.NotFound,
                    "no context found with given id");
            }
            
            return Result<QueryContextResult>.Ok(new QueryContextResult()
            {
                Content = contextData.Content,
                MediaType = contextData.MediaType,
                Text = contextData.Text
            });
        }

        public async Task<Result<Unit>> DeleteContext(Guid id)
        {
            var context = await this.dbContext.Contexts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (context == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context not found");
            }

            this.dbContext.Contexts.Remove(context);
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit>> DeleteTranslation(string projectName, string correlationId)
        {
            var translation = await this.dbContext.TranslationPairs
                .FirstOrDefaultAsync(t =>
                    t.Parent.Name == projectName &&
                    t.CorrelationId == correlationId);
            
            if (translation == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "translation not found");
            }

            this.dbContext.TranslationPairs.Remove(translation);
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit>> DeleteProject(string projectName)
        {
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(p => p.Name == projectName);

            if (project == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            this.dbContext.Projects.Remove(project);
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit>> UpdateTranslation(
            string projectName,
            string correlationId,
            string? source,
            string? target,
            Guid? context)
        {
            var translation = await this.dbContext.TranslationPairs
                .FirstOrDefaultAsync(t =>
                    t.Parent.Name == projectName &&
                    t.CorrelationId == correlationId);
            
            if (translation == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "translation not found");
            }

            var currentTime = this.currentTimeProvider.GetCurrentTime(); 
            
            if (source != null)
            {
                translation.Source = source;
                translation.ModificationTime = currentTime;
            }

            if (target != null)
            {
                translation.Target = target;
                translation.ModificationTime = currentTime;
            }

            if (context != null)
            {
                var contextExists = await this.dbContext.Contexts
                    .AnyAsync(c => c.Id == context.Value);

                if (!contextExists)
                {
                    return Result<Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "attempt to set to non-existing context");
                }
                
                translation.ContextId = context.Value;
                translation.ModificationTime = currentTime;
            }
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit>> AddProject(string projectName)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(
                p => p.Name == projectName);
            if (project != null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.Conflict,
                    "project with a given name already exists");
            }
            project = new Project()
            {
                Name = projectName
            };
            this.dbContext.Projects.Add(project);
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<AddTranslationsResult>> AddTranslations(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null)
            {
                return Result<AddTranslationsResult>.Failure(
                    HttpStatusCode.NotFound,
                    "no such project exists");
            }

            var inputContextIdList = translations
                .Select(translation => translation.Context)
                .OfType<Guid>();
            
            var inputTranslationIds = translations
                .Select(translation => translation.CorrelationId)
                .ToHashSet();

            if (inputTranslationIds.Count != translations.Count)
            {
                return Result<AddTranslationsResult>.Failure(
                    HttpStatusCode.BadRequest,
                    "attempting to add multiple translations with the same id");
            }

            var validContextIds = (await this.dbContext.Contexts
                    .Where(context => inputContextIdList.Contains(context.Id))
                    .Select(context => context.Id)
                    .ToListAsync())
                .ToHashSet();

            var alreadyExistingTranslationIds = (await this.dbContext.TranslationPairs
                    .Where(translation => translation.Parent.Name == projectName)
                    .Where(translation => inputTranslationIds.Contains(translation.CorrelationId))
                    .Select(translation => translation.CorrelationId)
                    .ToListAsync())
                .ToHashSet();

            if (!allowPartialAdd && alreadyExistingTranslationIds.Count != 0)
            {
                return Result<AddTranslationsResult>.Failure(
                    HttpStatusCode.BadRequest,
                    "there already exists a translation with given correlation id");
            }
            
            foreach (var translation in translations)
            {
                if (translation.Context != null &&
                    !validContextIds.Contains(translation.Context.Value))
                {
                    return Result<AddTranslationsResult>.Failure(
                        HttpStatusCode.BadRequest,
                        "the translation refers to non-existing context");
                }

                if (alreadyExistingTranslationIds.Contains(translation.CorrelationId))
                {
                    continue;
                }

                var currentTime = this.currentTimeProvider.GetCurrentTime();
                var model = new StoredModels.Translation()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = translation.CorrelationId,
                    Parent = project,
                    Target = translation.Target,
                    Source = translation.Source,
                    ContextId = translation.Context,
                    CreationTime = currentTime,
                    ModificationTime = currentTime
                };
                model.SearchVector = await this.dbContext.ToTsVector(analyzer.Normalize(translation.Source));
                this.dbContext.TranslationPairs.Add(model);
            }
            return Result<AddTranslationsResult>.Ok(new AddTranslationsResult()
            {
                NotAdded = alreadyExistingTranslationIds
            });
        }

        public async Task<Result<Unit>> AddContext(Guid id, byte[]? content, string? mediaType, string? text)
        {
            var context = await this.dbContext.Contexts.FindAsync(id);
            if (context != null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "request with such a guid exists");
            }

            if (text == null && mediaType == null && content == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "either text or content (with its media type) must be specified");
            }

            if (content != null && mediaType == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "if a request provides content, its media type must be specified");
            }
            
            if (content == null && mediaType != null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "no content provided");
            }

            AllowedMediaType? mediaTypeModel = null;
            if (mediaType != null)
            {
                mediaTypeModel = await this.dbContext.MediaTypes.FirstOrDefaultAsync(m => m.MediaType == mediaType);
                if (mediaTypeModel == null)
                {
                    return Result<Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "media type not acceptable");
                }
            }

            this.dbContext.Contexts.Add(new Context()
            {
                Id = id,
                Content = content,
                MediaType = mediaTypeModel,
                Text = text
            });
            
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task SaveChanges()
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}