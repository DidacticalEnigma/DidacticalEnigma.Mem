using System;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryTranslationResult
    {
        public QueryTranslationResult(
            string projectName,
            string source,
            string? target,
            string? highlighterSequence,
            string correlationId,
            Guid? context)
        {
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target;
            HighlighterSequence = highlighterSequence;
            CorrelationId = correlationId;
            Context = context;
        }

        public string ProjectName { get; }
        
        public string Source { get; }
        
        public string? Target { get; }
        
        public string? HighlighterSequence { get; }
        
        public string CorrelationId { get; }
        
        public Guid? Context { get; }
    }
}