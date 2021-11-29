using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddCategoriesParams
    {
        public IReadOnlyCollection<AddCategoryParams> Categories { get; set; }
    }

    public class AddCategoryParams
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
    }
}