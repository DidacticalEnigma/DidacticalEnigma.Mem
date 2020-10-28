using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryResult
    {
        public QueryResult(IEnumerable<QueryTranslationResult> translations)
        {
            Translations = translations ?? throw new ArgumentNullException(nameof(translations));
        }

        public IEnumerable<QueryTranslationResult> Translations { get; }
    }
}