using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class GenericDbResult
    {
        public string Result { get; set; }
        
        public int StatusCode { get; set; }

        public T Get<T>()
        {
            return JsonConvert.DeserializeObject<T>(Result);
        }
    }
}