using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;

namespace DidacticalEnigma.Mem.Translation.Services
{
    public interface ITranslationMemory
    {
        public Task<QueryResult> Query(string? projectName, string? correlationIdStart, string queryText, int limit = 50);

        Task AddProject(string projectName);

        Task AddTranslations(string projectName, IEnumerable<AddTranslation> translations);

        Task AddContext(Guid id, byte[]? context, string? mediaType, string? text);

        Task<QueryContextResult> GetContext(Guid id);
        
        Task SaveChanges();
    }
}