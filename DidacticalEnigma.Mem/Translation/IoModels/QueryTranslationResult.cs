using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryTranslationResult
    {
        public QueryTranslationResult(
            string projectName,
            string source,
            string? target,
            string? highlighterSequence,
            string? category,
            Guid? categoryId,
            string correlationId,
            QueryTranslationNotesResult? translationNotes,
            IDictionary<string, object>? associatedData)
        {
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target;
            HighlighterSequence = highlighterSequence;
            Category = category;
            CategoryId = categoryId;
            CorrelationId = correlationId;
            TranslationNotes = translationNotes;
            AssociatedData = associatedData;
        }

        public string ProjectName { get; }
        
        public string Source { get; }
        
        public string? Target { get; }
        
        public string? HighlighterSequence { get; }
        
        public string? Category { get; }
        
        public Guid? CategoryId { get; }

        public string CorrelationId { get; }
        
        public QueryTranslationNotesResult? TranslationNotes { get; }
        
        public IDictionary<string, object>? AssociatedData { get; }
    }
}