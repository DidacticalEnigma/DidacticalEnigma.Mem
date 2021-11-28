using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public static class DatabaseInitializer
    {
        private static object databaseLocker = new object();
        
        private static bool databaseInitialized = false;
        
        public static void InitializeDb(MemContext db)
        {
            lock (databaseLocker)
            {
                if (databaseInitialized)
                {
                    return;
                }
                
                db.Database.Migrate();
                
                Reset(db);

                QueryTranslationsTests.Initialize(db);
            
                db.SaveChanges();

                databaseInitialized = true;
            }
        }

        private static void Reset(MemContext db)
        {
            db.TranslationPairs.RemoveRange(db.TranslationPairs);
            db.Contexts.RemoveRange(db.Contexts);
            db.Projects.RemoveRange(db.Projects);
        }
    }
}