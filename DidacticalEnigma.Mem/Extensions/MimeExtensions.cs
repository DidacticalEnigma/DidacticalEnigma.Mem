namespace DidacticalEnigma.Mem.Extensions
{
    public static class MimeExtensions
    {
        public static bool IsImage(string? mime)
        {
            switch (mime)
            {
                case "image/png": return true;
                case "image/jpeg": return true;
                default: return false;
            }
        }
    }
}