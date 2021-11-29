using System.Collections.Generic;

namespace DidacticalEnigma.Mem.DatabaseModels
{
    public class AllowedMediaType
    {
        public int Id { get; set; }
        
        public string MediaType { get; set; }
        
        public string Extension { get; set; }

        public static IEnumerable<AllowedMediaType> GetAllowedMediaTypes()
        {
            yield return new AllowedMediaType() {Id = 1, MediaType = "image/jpeg", Extension = "jpg"};
            yield return new AllowedMediaType() {Id = 2, MediaType = "image/png", Extension = "png"};
        }
    }
}