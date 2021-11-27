using System.Linq;

namespace DidacticalEnigma.Mem.Migrations
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                args.FirstOrDefault()
                ?? "Host=localhost;Port=5432;Database=didacticalenigma;Username=didacticalenigma;Password=tt9lFUsiLTenOMLTNr4k";

            var updateLog = new DbUp.Engine.Output.ConsoleUpgradeLog();
            
            Database.Migrations.MigrateToLatest(connectionString, updateLog);
        }
    }
}