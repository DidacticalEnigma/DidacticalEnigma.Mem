namespace DidacticalEnigma.Mem.Configurations
{
    public class AuthConfiguration
    {
        public string Authority { get; set; }
        
        public string Audience { get; set; }
        
        public bool AnonymousUsersCanReadTranslations { get; set; }
        
        public bool AnonymousUsersCanReadContexts { get; set; }
    }
}