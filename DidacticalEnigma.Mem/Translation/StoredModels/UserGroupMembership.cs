using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class UserGroupMembership
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}