using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryTranslationsResult
    {
        public QueryTranslationsResult(
            IEnumerable<QueryTranslationResult> translations,
            string? paginationToken,
            DateTime queryTime)
        {
            Translations = translations ?? throw new ArgumentNullException(nameof(translations));
            PaginationToken = paginationToken;
            QueryTime = queryTime;
        }

        public IEnumerable<QueryTranslationResult> Translations { get; }
        
        public string? PaginationToken { get; }
        
        public DateTime QueryTime { get; }
    }
}