namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class ContributorMembership
    {
        public int Id { get; set; }
        
        public User User { get; set; }
        
        public string UserId { get; set; }
        
        public Project Project { get; set; }
        
        public int ProjectId { get; set; }
    }
}