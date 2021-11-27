using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Translation.DbModels;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using Newtonsoft.Json;
using Npgsql;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public class TranslationMemory : ITranslationMemory, IDisposable
    {
        private readonly DbConnection connection;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        private readonly DbTransaction transaction;

        public TranslationMemory(
        DbConnection connection,
        IMorphologicalAnalyzer<IpadicEntry> analyzer,
        ICurrentTimeProvider currentTimeProvider)
        {
            this.connection = connection;
            connection.Open();
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
            this.transaction = connection.BeginTransaction();
        }
        
        public async Task<Result<QueryResult>> Query(string? projectName, string? correlationIdStart, string? queryText, int limit = 50)
        {
            if (projectName == null && correlationIdStart == null && queryText == null)
            {
                return Result<QueryResult>.Failure(
                    HttpStatusCode.BadRequest,
                    "one of: projectName, correlationId, queryText must be provided");
            }
            limit = Math.Min(limit, 250);

            var parameters = new QueryTranslationsDbParams()
            {
                InputProjectName = projectName,
                InputCorrelationId = correlationIdStart,
                NormalizedQueryText = queryText != null ? analyzer.Normalize(queryText) : null,
                Limit = limit
            };

            var results =
                await this.connection.QueryAsync<QueryTranslationsDbResultEntry>(
                    DapperExtensions.SqlForFunction("QueryTranslations", parameters),
                    parameters,
                    transaction);
            
            return Result<QueryResult>.Ok(new QueryResult(results.Select(
                result =>
                {
                    var (source, highlighter) = queryText != null
                        ? analyzer.Highlight(result.Source, queryText)
                        : (result.Source, null);
                    return new QueryTranslationResult(
                        projectName: result.ParentName,
                        source: source,
                        target: result.Target,
                        highlighterSequence: highlighter,
                        correlationId: result.CorrelationId,
                        context: result.Context);
                })));
        }

        public async Task<Result<Unit>> AddProject(string projectName)
        {
            var parameters = new AddProjectDbParams()
            {
                InputProjectName = projectName
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("AddProject", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.Conflict,
                        "project with a given name already exists");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<AddTranslationsResult>> AddTranslations(string projectName, IReadOnlyCollection<AddTranslationParams> translations, bool allowPartialAdd = false)
        {
            var parameters = new AddTranslationsDbParams()
            {
                Translations = JsonConvert.SerializeObject(
                    translations
                        .Select(translation => new AddTranslationsDbNewEntry()
                        {
                            Context = translation.Context,
                            Id = Guid.NewGuid(),
                            Source = translation.Source,
                            Target = translation.Target,
                            CorrelationId = translation.CorrelationId,
                            NormalizedSource = analyzer.Normalize(translation.Source)
                        })),
                CurrentTime = currentTimeProvider.GetCurrentTime(),
                AllowPartialAdd = allowPartialAdd,
                InputProjectName = projectName
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("AddTranslations", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<AddTranslationsResult>.Ok(result.Get<AddTranslationsResult>());
                case 1:
                    return Result<AddTranslationsResult>.Failure(
                        HttpStatusCode.NotFound,
                        "no such project exists");
                case 2:
                    return Result<AddTranslationsResult>.Failure(
                        HttpStatusCode.BadRequest,
                        "attempting to add multiple translations with the same id");
                case 3:
                    return Result<AddTranslationsResult>.Failure(
                        HttpStatusCode.BadRequest,
                        "the translation refers to non-existing context");
                case 4:
                    return Result<AddTranslationsResult>.Failure(
                        HttpStatusCode.BadRequest,
                        "there already exists a translation with given correlation id");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<Unit>> AddContext(Guid id, byte[]? content, string? mediaType, string? text)
        {
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
            
            var parameters = new AddContextDbParams()
            {
                InputContent = content,
                InputText = text,
                InputContextId = id,
                InputMediaType = mediaType
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("AddContext", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "media type not acceptable");
                case 2:
                    return Result<Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "request with such a guid exists");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<QueryContextResult>> GetContext(Guid id)
        {
            var parameters = new GetContextDbParams()
            {
                InputContextId = id
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("GetContext", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<QueryContextResult>.Ok(result.Get<QueryContextResult>());
                case 1:
                    return Result<QueryContextResult>.Failure(
                        HttpStatusCode.NotFound,
                        "no context found with given id");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<Unit>> DeleteContext(Guid id)
        {
            var parameters = new DeleteContextDbParams()
            {
                InputContextId = id
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("DeleteContext", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.NotFound,
                        "context not found");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<Unit>> DeleteTranslation(string projectName, string correlationId)
        {
            var parameters = new DeleteTranslationDbParams()
            {
                InputCorrelationId = correlationId,
                InputProjectName = projectName
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("DeleteTranslation", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.NotFound,
                        "translation not found");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<Unit>> DeleteProject(string projectName)
        {
            var parameters = new DeleteProjectDbParams()
            {
                InputProjectName = projectName
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("DeleteProject", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.NotFound,
                        "project not found");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task<Result<Unit>> UpdateTranslation(string projectName, string correlationId, string? source, string? target, Guid? context)
        {
            var parameters = new UpdateTranslationDbParams()
            {
                InputProjectName = projectName,
                CurrentTime = this.currentTimeProvider.GetCurrentTime(),
                InputSource = source,
                InputTarget = target,
                InputNormalizedSource = source != null ? analyzer.Normalize(source) : null,
                
            };

            var result = await this.connection.QuerySingleAsync<GenericDbResult>(
                DapperExtensions.SqlForFunction("DeleteProject", parameters),
                parameters,
                transaction);

            switch (result.StatusCode)
            {
                case 0:
                    return Result<Unit>.Ok(result.Get<Unit>());
                case 1:
                    return Result<Unit>.Failure(
                        HttpStatusCode.NotFound,
                        "project not found");
                default:
                    throw new InvalidDataException();
            }
        }

        public async Task SaveChanges()
        {
            await this.transaction.CommitAsync();
        }

        public void Dispose()
        {
            this.transaction.Dispose();
        }
    }
}