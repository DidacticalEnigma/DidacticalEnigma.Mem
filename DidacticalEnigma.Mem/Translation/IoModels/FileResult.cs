using System;
using System.IO;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class FileResult
    {
        public Stream Content { get; set; }
        
        
        public string MediaType { get; set; }
        
        
        public string FileName { get; set; }
        
        public DateTime LastModified { get; set; }
    }
}