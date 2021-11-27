namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class QueryTranslationsDbParams
    {
        public string? InputProjectName { get; init; }
        
        public string? InputCorrelationId { get; init; }
        
        public string? NormalizedQueryText { get; init; }
        
        public int Limit { get; init; }
    }
}