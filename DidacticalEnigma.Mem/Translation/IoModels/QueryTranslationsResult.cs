using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryTranslationsResult
    {
        public QueryTranslationsResult(
            IEnumerable<QueryTranslationResult> translations,
            string? paginationToken = null)
        {
            Translations = translations ?? throw new ArgumentNullException(nameof(translations));
            PaginationToken = paginationToken;
        }

        public IEnumerable<QueryTranslationResult> Translations { get; }
        
        public string? PaginationToken { get; }
    }
}