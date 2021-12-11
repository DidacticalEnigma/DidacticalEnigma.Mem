using System.Collections.Generic;

namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class Project
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public IReadOnlyCollection<Translation> Translations { get; set; }
        
        public IReadOnlyCollection<Category> Categories { get; set; }
        
        public User Owner { get; set; }
        
        public string OwnerId { get; set; }
        
        public IReadOnlyCollection<ContributorMembership> Contributors { get; set; }
        
        public IReadOnlyCollection<ContributorInvitation> Invitations { get; set; }
        
        public bool PublicallyReadable { get; set; }
    }
}