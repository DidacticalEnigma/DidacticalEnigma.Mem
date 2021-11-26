using System.Linq;

namespace DidacticalEnigma.Mem.Migrations
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                args.FirstOrDefault()
                ?? "Host=localhost;Port=5432;Database=detest;Username=detest;Password=POSTGRES_PASSWORD_GOES_HERE";

            var updateLog = new DbUp.Engine.Output.ConsoleUpgradeLog();
            
            Database.Migrations.MigrateToLatest(connectionString, updateLog);
        }
    }
}