using System;

namespace DidacticalEnigma.Mem.Translation.DbModels
{
    public class AddTranslationsDbParams
    {
        public string InputProjectName { get; init; }
        
        public DateTime CurrentTime { get; init; }
        
        public bool AllowPartialAdd { get; init; }
        
        public string Translations { get; set; }
    }
}