namespace DidacticalEnigma.Mem.Models
{
    public class TranslationEntry
    {
        public string Source { get; init; }
        
        public string? Target { get; init; }
        
        public string Project { get; init; }
        
        public string CorrelationId { get; init; }
        
        public string? Highlight { get; init; }
    }
}