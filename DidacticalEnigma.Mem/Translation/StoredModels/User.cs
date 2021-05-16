using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class User
    {
        public static Guid AnonymousUserId = new Guid("32674391-9E28-422B-9DD7-3EDEFADB3417");
        
        public static Guid AdminUserId = new Guid("B97335BD-6670-414F-B7E7-C7BE511B4A6C");
        
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public bool IsSpecialUser { get; set; }
        
        public IReadOnlyCollection<UserGroupMembership> Groups { get; set; }
    }
}