using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            db.Database.OpenConnection();
            var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            var lobManager = new NpgsqlLargeObjectManager(conn);
            var lobs = db.Contexts.Select(context => context.ContentObjectId).ToList();
            using (var transaction = conn.BeginTransaction())
            {
                foreach (var oid in lobs)
                {
                    if (oid != null)
                    {
                        try
                        {
                            lobManager.Unlink(oid.Value);
                        }
                        catch (Exception e)
                        {
                            // do nothing when the object does not exist
                        }
                    }
                }
                transaction.Commit();
            }

            db.TranslationPairs.RemoveRange(db.TranslationPairs);
            db.Contexts.RemoveRange(db.Contexts);
            db.Projects.RemoveRange(db.Projects);
        }
    }
}