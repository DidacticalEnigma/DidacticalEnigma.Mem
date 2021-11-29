using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.IoModels;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public interface ITranslationMemory
    {
        Task<Result<QueryTranslationsResult, Unit>> Query(
            string? projectName,
            string? correlationIdStart,
            string? queryText,
            string? category,
            string? paginationToken = null,
            int? limit = null);

        Task<Result<Unit, Unit>> AddProject(
            string projectName);

        Task<Result<AddTranslationsResult, Unit>> AddTranslations(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false);

        Task<Result<Unit, Unit>> AddContext(Guid id,
            string correlationId,
            string projectName,
            Stream? context,
            string? mediaType,
            string? text);

        Task<Result<QueryContextsResult, Unit>> GetContexts(
            Guid? id,
            string? projectName,
            string? correlationId);
        
        Task<Result<FileResult, Unit>> GetContextData(
            Guid id);

        Task<Result<Unit, Unit>> DeleteContext(
            Guid id);

        Task<Result<Unit, Unit>> DeleteTranslation(
            string projectName,
            string correlationId);
        
        Task<Result<Unit, Unit>> DeleteProject(
            string projectName);

        Task<Result<Unit, QueryTranslationResult>> UpdateTranslation(
            string projectName,
            string correlationId,
            UpdateTranslationParams uploadParams);
        
        Task<Result<QueryCategoriesResult, Unit>> QueryCategories(
            string projectName);
        
        Task<Result<Unit, Unit>> AddCategories(
            string projectName,
            AddCategoriesParams categoriesParams);
        
        Task<Result<Unit, Unit>> DeleteCategory(
            Guid categoryId);
    }
}