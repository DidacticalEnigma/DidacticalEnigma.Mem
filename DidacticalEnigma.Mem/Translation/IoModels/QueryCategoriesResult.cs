using System;
using System.Collections.Generic;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryCategoriesResult
    {
        public IReadOnlyCollection<QueryCategoryResult> Categories { get; set; }
    }

    public class QueryCategoryResult
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
    }
}