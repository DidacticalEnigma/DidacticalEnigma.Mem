using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DidacticalEnigma.Mem.IntegrationTests
{
    public static class DatabaseInitializer
    {
        private static object databaseLocker = new object();
        
        private static bool databaseInitialized = false;
        
        public static void InitializeDb(IServiceProvider serviceProvider)
        {
            lock (databaseLocker)
            {
                if (databaseInitialized)
                {
                    return;
                }

                var db = serviceProvider.GetRequiredService<MemContext>();
                
                db.Database.Migrate();
                
                Reset(db);

                QueryTranslationsTests.Initialize(db);
            
                db.SaveChanges();

                databaseInitialized = true;
            }
        }

        private static void Reset(MemContext db)
        {
            db.Memberships.RemoveRange(db.Memberships);
            db.Invitations.RemoveRange(db.Invitations);
            db.TranslationPairs.RemoveRange(db.TranslationPairs);
            db.Contexts.RemoveRange(db.Contexts);
            db.Categories.RemoveRange(db.Categories);
            db.Projects.RemoveRange(db.Projects);
            db.Users.RemoveRange(db.Users);
        }
    }
}