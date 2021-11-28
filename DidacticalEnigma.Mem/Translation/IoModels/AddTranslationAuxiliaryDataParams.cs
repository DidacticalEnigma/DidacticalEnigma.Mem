using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class AddTranslationAuxiliaryDataParams
    {
        [JsonExtensionData]
        public IDictionary<string, object> Data { get; set; }
    }
}