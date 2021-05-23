using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public interface ITranslationMemory
    {
        Task<Result<QueryResult>> Query(string? projectName, string? correlationIdStart, string queryText, int limit = 50);

        Task<Result<Unit>> AddProject(string projectName);

        Task<Result<Unit>> AddTranslations(string projectName, IReadOnlyCollection<AddTranslationParams> translations);

        Task<Result<Unit>> AddContext(Guid id, byte[]? context, string? mediaType, string? text);

        Task<Result<QueryContextResult>> GetContext(Guid id);

        Task<Result<Unit>> DeleteContext(Guid id);

        Task<Result<Unit>> DeleteTranslation(string projectName, string correlationId);
        
        Task<Result<Unit>> DeleteProject(string projectName);
        
        Task SaveChanges();
    }
}