using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace DidacticalEnigma.Mem.Pages
{
    public class AuthorizeApplicationModel
    {
        public string ApplicationName { get; set; }
        
        public string Scope { get; set; }
        
        public IEnumerable<KeyValuePair<string, StringValues>> Params { get; set; }
    }
}