using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.IoModels;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public interface ITranslationMemory
    {
        Task<Result<QueryTranslationsResult>> Query(
            string? projectName,
            string? correlationIdStart,
            string queryText,
            string? paginationToken = null,
            int limit = 50);

        Task<Result<Unit>> AddProject(
            string projectName);

        Task<Result<AddTranslationsResult>> AddTranslations(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false);

        Task<Result<Unit>> AddContext(Guid id,
            string correlationId,
            string projectName,
            Stream? context,
            string? mediaType,
            string? text);

        Task<Result<QueryContextResult>> GetContext(
            Guid id);

        Task<Result<QueryContextsResult>> GetContexts(
            string correlationId);
        
        Task<Result<FileResult>> GetContextData(
            Guid id);

        Task<Result<Unit>> DeleteContext(
            Guid id);

        Task<Result<Unit>> DeleteTranslation(
            string projectName,
            string correlationId);
        
        Task<Result<Unit>> DeleteProject(
            string projectName);

        Task<Result<Unit>> UpdateTranslation(
            string projectName,
            string correlationId,
            string? source,
            string? target);
    }
}