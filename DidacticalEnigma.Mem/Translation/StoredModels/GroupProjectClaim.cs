namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class GroupProjectClaim
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        
        public int GroupId { get; set; }
        public Group Group { get; set; }
        
        public bool CanAddTranslations { get; set; }
        
        public bool CanDeleteTranslations { get; set; }
        
        public bool CanReadTranslations { get; set; }
    }
}