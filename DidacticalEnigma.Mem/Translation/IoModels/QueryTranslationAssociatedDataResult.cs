using System.Collections.Generic;
using Newtonsoft.Json;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class QueryTranslationAssociatedDataResult
    {
        [JsonExtensionData]
        public IDictionary<string, object> Data { get; set; }
    }
}