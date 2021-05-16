using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Group
    {
        public int Id { get; set; }
        
        public string GroupName { get; set; }
        
        public bool CanAddContexts { get; set; }
        
        public bool CanDeleteContexts { get; set; }
        
        public bool CanAddProjects { get; set; }
        
        public bool CanDeleteProjects { get; set; }
        
        public IReadOnlyCollection<UserGroupMembership> Users { get; set; }
        
        public IReadOnlyCollection<GroupProjectClaim> ProjectClaims { get; set; }
    }
}