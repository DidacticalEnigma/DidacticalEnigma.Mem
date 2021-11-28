using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
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

        public async Task<Result<QueryTranslationsResult>> Query(
            string? projectName,
            string? correlationIdStart,
            string? queryText,
            string? paginationToken = null,
            int limit = 50)
        {
            if (projectName == null && correlationIdStart == null && queryText == null)
            {
                return Result<QueryTranslationsResult>.Failure(
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
                        correlationId: selection.CorrelationId,
                        translationNotes: Map(selection.Notes),
                        associatedData: selection.AssociatedData != null
                            ? JsonSerializer.Deserialize<QueryTranslationAssociatedDataResult>(selection.AssociatedData)
                            : null);
                });
            
            return Result<QueryTranslationsResult>.Ok(new QueryTranslationsResult(results));

            QueryTranslationNotesResult? Map(NotesCollection? notes)
            {
                if (notes == null)
                    return null;

                return new QueryTranslationNotesResult()
                {
                    Gloss = notes.GlossNotes
                        .Select(n => new IoGlossNote()
                        {
                            Explanation = n.Text,
                            Foreign = n.Foreign
                        })
                        .ToList(),
                    Normal = notes.NormalNote
                        .Select(n => new IoNormalNote()
                        {
                            Text = n.Text,
                            SideText = n.SideComment
                        })
                        .ToList()
                };
            }
        }
        
        public async Task<Result<QueryContextsResult>> GetContexts(string correlationId)
        {
            var contexts = (await this.dbContext.Contexts
                    .Where(context => correlationId.StartsWith(context.CorrelationId))
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
            
            return Result<QueryContextsResult>.Ok(new QueryContextsResult()
            {
                Contexts = contexts
            });
        }

        public async Task<Result<QueryContextResult>> GetContext(Guid id)
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
                return Result<QueryContextResult>.Failure(
                    HttpStatusCode.NotFound,
                    "no context found with given id");
            }

            await this.dbContext.SaveChangesAsync();
            return Result<QueryContextResult>.Ok(new QueryContextResult()
            {
                Id = id,
                CorrelationId = contextData.CorrelationId,
                MediaType = contextData.MediaType,
                Text = contextData.Text,
                ProjectName = contextData.ProjectName,
                HasData = contextData.HasData,
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
            
            await this.dbContext.SaveChangesAsync();
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
            
            await this.dbContext.SaveChangesAsync();
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
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<Unit>> UpdateTranslation(
            string projectName,
            string correlationId,
            string? source,
            string? target)
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
                translation.SearchVector = await this.dbContext.ToTsVector(source);
                translation.ModificationTime = currentTime;
            }

            if (target != null)
            {
                translation.Target = target;
                translation.ModificationTime = currentTime;
            }

            await this.dbContext.SaveChangesAsync();
            return Result<Unit>.Ok(Unit.Value);
        }

        public async Task<Result<FileResult>> GetContextData(Guid id)
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
                return Result<FileResult>.Failure(
                    HttpStatusCode.NotFound,
                    "context not found");
            }

            if (contextData.ContentObjectId == null || contextData.MediaType == null)
            {
                return Result<FileResult>.Failure(
                    HttpStatusCode.NotFound,
                    "context has no associated binary data");
            }
            
            var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();

            var lobManager = new NpgsqlLargeObjectManager(connection);

            return Result<FileResult>.Ok(new FileResult()
            {
                Content = await lobManager.OpenReadAsync(contextData.ContentObjectId.Value),
                FileName = $"{contextData.Id}.{contextData.Extension}",
                MediaType = contextData.MediaType
            });
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
            
            await this.dbContext.SaveChangesAsync();
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

            var inputTranslationIds = translations
                .Select(translation => translation.CorrelationId)
                .ToHashSet();

            if (inputTranslationIds.Count != translations.Count)
            {
                return Result<AddTranslationsResult>.Failure(
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
                return Result<AddTranslationsResult>.Failure(
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
                        {project.Id},
                        {currentTime},
                        {currentTime},
                        {JsonSerializer.Serialize(Map(translation.TranslationNotes))}::jsonb,
                        {JsonSerializer.Serialize(translation.AuxiliaryData)}::jsonb);");
            }
            
            await this.dbContext.SaveChangesAsync();
            return Result<AddTranslationsResult>.Ok(new AddTranslationsResult()
            {
                NotAdded = alreadyExistingTranslationIds
            });

            NotesCollection? Map(AddTranslationNotesParams? notes)
            {
                if (notes == null)
                    return null;

                return new NotesCollection()
                {
                    GlossNotes = notes.Gloss
                        .Select(n => new GlossNote()
                        {
                            Foreign = n.Foreign,
                            Text = n.Explanation
                        })
                        .ToList(),
                    NormalNote = notes.Normal
                        .Select(n => new NormalNote()
                        {
                            Text = n.Text,
                            SideComment = n.SideText
                        })
                        .ToList()
                };
            }
        }

        public async Task<Result<Unit>> AddContext(
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
            
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(project => project.Name == projectName);

            if (project == null)
            {
                return Result<Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "project does not exist");
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

            uint? contentObjectId = null;
            if (content != null)
            {
                var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();

                var lobManager = new NpgsqlLargeObjectManager(connection);
                var contentOid = await lobManager.CreateAsync(0);

                using (var stream = await lobManager.OpenReadWriteAsync(contentOid))
                {
                    await content.CopyToAsync(stream);
                }

                contentObjectId = contentOid;
            }

            this.dbContext.Contexts.Add(new Context()
            {
                Id = id,
                ContentObjectId = contentObjectId,
                MediaType = mediaTypeModel,
                Text = text,
                CorrelationId = correlationId,
                Project = project
            });
            
            await this.dbContext.SaveChangesAsync();
            return Result<Unit>.Ok(Unit.Value);
        }
    }
}