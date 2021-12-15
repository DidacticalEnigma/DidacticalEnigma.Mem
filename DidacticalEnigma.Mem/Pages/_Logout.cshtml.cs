using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace DidacticalEnigma.Mem.Pages
{
    public class LogoutModel
    {
        public IEnumerable<KeyValuePair<string, StringValues>> Params { get; set; }
    }
}