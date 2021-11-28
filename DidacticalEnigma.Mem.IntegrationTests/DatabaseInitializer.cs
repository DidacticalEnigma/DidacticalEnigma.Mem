using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public static class DatabaseInitializer
    {
        public static void InitializeDb(MemContext db)
        {
            Reset(db);

            QueryTranslationsTests.Initialize(db);
            
            db.SaveChanges();
        }

        public static void Reset(MemContext db)
        {
            db.TranslationPairs.RemoveRange(db.TranslationPairs);
            db.Contexts.RemoveRange(db.Contexts);
            db.Projects.RemoveRange(db.Projects);
        }
    }
}