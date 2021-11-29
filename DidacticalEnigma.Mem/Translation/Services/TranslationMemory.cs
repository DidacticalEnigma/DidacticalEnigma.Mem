using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Mappings;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        public async Task<Result<QueryTranslationsResult, Unit>> Query(
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
        
        public async Task<Result<QueryContextsResult, Unit>> GetContexts(Guid? id, string? projectName, string? correlationId)
        {
            if (id == null && correlationId == null && projectName == null)
            {
                return Result<QueryContextsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "no parameters are provided");
            }
            
            if (id != null && (correlationId != null || projectName != null))
            {
                return Result<QueryContextsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "the search can be done either by direct id, or by projectName/correlationId combination");
            }
            
            var filteredContexts = this.dbContext.Contexts.AsQueryable();

            if (id != null)
            {
                filteredContexts = filteredContexts.Where(context => context.Id == id);
            }
            
            if (correlationId != null && projectName != null)
            {
                filteredContexts = filteredContexts.Where(context =>
                    context.Project.Name == projectName &&
                    context.CorrelationId.StartsWith(correlationId));
            }
            
            var contexts = (await filteredContexts
                    .Select(context => new
                    {
                        Id = context.Id,
                        HasData = context.ContentObjectId != null,
                        MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                        Text = context.Text,
                        CorrelationId = context.CorrelationId,
                        ProjectName = context.Project.Name
                    })
                    .ToListAsync())
                .Select(contextData => new QueryContextResult()
                {
                    Id = contextData.Id,
                    CorrelationId = contextData.CorrelationId,
                    MediaType = contextData.MediaType,
                    Text = contextData.Text,
                    ProjectName = contextData.ProjectName,
                    HasData = contextData.HasData,
                })
                .ToList();
            
            return Result<QueryContextsResult, Unit>.Ok(new QueryContextsResult()
            {
                Contexts = contexts
            });
        }

        public async Task<Result<QueryContextResult, Unit>> GetContext(Guid id)
        {
            var contextData = await this.dbContext.Contexts
                .Where(context => context.Id == id)
                .Select(context => new {
                    HasData = context.ContentObjectId != null,
                    MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                    Text = context.Text,
                    CorrelationId = context.CorrelationId,
                    ProjectName = context.Project.Name
                })
                .FirstOrDefaultAsync();

            if (contextData == null)
            {
                return Result<QueryContextResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "no context found with given id");
            }

            await this.dbContext.SaveChangesAsync();
            return Result<QueryContextResult, Unit>.Ok(new QueryContextResult()
            {
                Id = id,
                CorrelationId = contextData.CorrelationId,
                MediaType = contextData.MediaType,
                Text = contextData.Text,
                ProjectName = contextData.ProjectName,
                HasData = contextData.HasData,
            });
        }

        public async Task<Result<Unit, Unit>> DeleteContext(Guid id)
        {
            var context = await this.dbContext.Contexts
                .FirstOrDefaultAsync(c => c.Id == id);

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

        public async Task<Result<Unit, Unit>> DeleteTranslation(string projectName, string correlationId)
        {
            var translation = await this.dbContext.TranslationPairs
                .FirstOrDefaultAsync(t =>
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

        public async Task<Result<Unit, Unit>> DeleteProject(string projectName)
        {
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(p => p.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "project not found");
            }

            this.dbContext.Projects.Remove(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit, QueryTranslationResult>> UpdateTranslation(
            string projectName,
            string correlationId,
            UpdateTranslationParams uploadParams)
        {
            var translation = await this.dbContext.TranslationPairs
                .Include(t => t.Category)
                .Include(t => t.Parent)
                .FirstOrDefaultAsync(t =>
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

        public async Task<Result<QueryCategoriesResult, Unit>> QueryCategories(string projectName)
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

        public async Task<Result<Unit, Unit>> AddCategories(
            string projectName,
            AddCategoriesParams categoriesParams)
        {
            var projectId = await this.dbContext.Projects
                .Where(project => project.Name == projectName)
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

        public async Task<Result<Unit, Unit>> DeleteCategory(Guid categoryId)
        {
            var category = await this.dbContext.Categories
                .FirstOrDefaultAsync(category => category.Id == categoryId);
            
            if (category == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "category not found");
            }

            this.dbContext.Categories.Remove(category);
            await this.dbContext.SaveChangesAsync();
            
            return Result<Unit, Unit>.Ok(Unit.Value);
        }

        public async Task<Result<FileResult, Unit>> GetContextData(Guid id)
        {
            var contextData = await this.dbContext.Contexts
                .Select(context => new
                {
                    Id = context.Id,
                    ContentObjectId = context.ContentObjectId,
                    MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                    Extension = context.MediaType != null ? context.MediaType.Extension : null,
                })
                .FirstOrDefaultAsync(contextData => contextData.Id == id);
            
            if (contextData == null)
            {
                return Result<FileResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context not found");
            }

            if (contextData.ContentObjectId == null || contextData.MediaType == null)
            {
                return Result<FileResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context has no associated binary data");
            }
            
            await this.dbContext.Database.OpenConnectionAsync();
            var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();
            // we have to begin a transaction, in order for LOB access to work
            // (otherwise we get a "invalid large-object descriptor: 0" error)
            // but we can't dispose it in this method, so we wrap the created stream
            // in a wrapper that disposes another object after disposing the stream
            var transaction = await this.dbContext.Database.BeginTransactionAsync();

            var lobManager = new NpgsqlLargeObjectManager(connection);

            return Result<FileResult, Unit>.Ok(new FileResult()
            {
                Content = new DisposingStream(
                    await lobManager.OpenReadAsync(contextData.ContentObjectId.Value),
                    transaction),
                FileName = $"{contextData.Id}.{contextData.Extension}",
                MediaType = contextData.MediaType
            });
        }

        public async Task<Result<Unit, Unit>> AddProject(string projectName)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(
                p => p.Name == projectName);
            if (project != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.Conflict,
                    "project with a given name already exists");
            }
            project = new Project()
            {
                Name = projectName
            };
            this.dbContext.Projects.Add(project);
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit, Unit>.Ok(Unit.Value);
        }

        public async Task<Result<AddTranslationsResult, Unit>> AddTranslations(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "no such project exists");
            }

            var inputTranslationIds = translations
                .Select(translation => translation.CorrelationId)
                .ToHashSet();

            if (inputTranslationIds.Count != translations.Count)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "attempting to add multiple translations with the same id");
            }

            var alreadyExistingTranslationIds = (await this.dbContext.TranslationPairs
                    .Where(translation => translation.Parent.Name == projectName)
                    .Where(translation => inputTranslationIds.Contains(translation.CorrelationId))
                    .Select(translation => translation.CorrelationId)
                    .ToListAsync())
                .ToHashSet();

            if (!allowPartialAdd && alreadyExistingTranslationIds.Count != 0)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "there already exists a translation with given correlation id");
            }
            
            var currentTime = this.currentTimeProvider.GetCurrentTime();
            
            foreach (var translation in translations)
            {
                if (alreadyExistingTranslationIds.Contains(translation.CorrelationId))
                {
                    continue;
                }

                await this.dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO ""TranslationPairs"" (
                        ""Id"",
                        ""CorrelationId"",
                        ""Source"",
                        ""SearchVector"",
                        ""Target"",
                        ""CategoryId""
                        ""ParentId"",
                        ""CreationTime"",
                        ""ModificationTime"",
                        ""Notes"",
                        ""AssociatedData"")
                    VALUES (
                        {Guid.NewGuid()},
                        {translation.CorrelationId},
                        {translation.Source},
                        to_tsvector('simple', {analyzer.Normalize(translation.Source)}),
                        {translation.Target},
                        {translation.CategoryId},
                        {project.Id},
                        {currentTime},
                        {currentTime},
                        {JsonSerializer.Serialize(Mapper.Map(translation.TranslationNotes))}::jsonb,
                        {JsonSerializer.Serialize(translation.AssociatedData)}::jsonb);");
            }
            
            await this.dbContext.SaveChangesAsync();
            return Result<AddTranslationsResult, Unit>.Ok(new AddTranslationsResult()
            {
                NotAdded = alreadyExistingTranslationIds
            });
        }

        public async Task<Result<Unit, Unit>> AddContext(
            Guid id,
            string correlationId,
            string projectName,
            Stream? content,
            string? mediaType,
            string? text)
        {
            var context = await this.dbContext.Contexts.FindAsync(id);
            if (context != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "request with such a guid exists");
            }

            if (text == null && mediaType == null && content == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "either text or content (with its media type) must be specified");
            }
            
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(project => project.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "project does not exist");
            }

            if (content != null && mediaType == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "if a request provides content, its media type must be specified");
            }
            
            if (content == null && mediaType != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "no content provided");
            }

            AllowedMediaType? mediaTypeModel = null;
            if (mediaType != null)
            {
                mediaTypeModel = await this.dbContext.MediaTypes.FirstOrDefaultAsync(m => m.MediaType == mediaType);
                if (mediaTypeModel == null)
                {
                    return Result<Unit, Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "media type not acceptable");
                }
            }

            uint? contentObjectId = null;
            
            await this.dbContext.Database.OpenConnectionAsync();
            var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                if (content != null)
                {
                    var lobManager = new NpgsqlLargeObjectManager(connection);
                    var contentOid = await lobManager.CreateAsync(0);

                    using (var stream = await lobManager.OpenReadWriteAsync(contentOid))
                    {
                        await content.CopyToAsync(stream);
                    }

                    contentObjectId = contentOid;
                }

                await this.dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO ""Contexts"" (
                        ""Id"",
                        ""ProjectId"",
                        ""CorrelationId"",
                        ""Text"",
                        ""ContentObjectId"",
                        ""MediaTypeId"")
                    VALUES (
                        {id},
                        {project.Id},
                        {correlationId},
                        {text},
                        {contentObjectId},
                        {mediaTypeModel?.Id});");

                await transaction.CommitAsync();
            }
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}