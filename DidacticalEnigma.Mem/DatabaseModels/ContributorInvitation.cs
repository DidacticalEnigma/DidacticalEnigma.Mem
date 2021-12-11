namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class ContributorInvitation
    {
        public int Id { get; set; }
        
        public User InvitedUser { get; set; }
        
        public string InvitedUserId { get; set; }
        
        public User InvitingUser { get; set; }
        
        public string InvitingUserId { get; set; }
        
        public Project Project { get; set; }
        
        public int ProjectId { get; set; }

        public ContributorMembership CreateMembership()
        {
            return new ContributorMembership()
            {
                ProjectId = this.ProjectId,
                UserId = this.InvitedUserId
            };
        }
    }
}