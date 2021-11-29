using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.StoredModels
{
    public class Project
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public IReadOnlyCollection<Translation> Translations { get; set; }
        
        public IReadOnlyCollection<Category> Categories { get; set; }
    }
}