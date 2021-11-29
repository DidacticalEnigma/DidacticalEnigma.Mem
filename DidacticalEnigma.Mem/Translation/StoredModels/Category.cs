using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Category
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public Project Parent { get; set; }
        
        public int ParentId { get; set; }
        
        public IReadOnlyCollection<Translation> Translations { get; set; }
    }
}