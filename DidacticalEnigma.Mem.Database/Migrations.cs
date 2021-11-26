using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Helpers;

namespace DidacticalEnigma.Mem.Database
{
    public static class Migrations
    {
        public static bool MigrateToLatest(string connectionString, IUpgradeLog upgradeLog)
        {
            var sprocUpgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(
                        Assembly.GetExecutingAssembly(),
                        s => s.StartsWith("DidacticalEnigma.Mem.Database.Code",
                            StringComparison.InvariantCulture))
                    .JournalTo(new NullJournal())
                    .LogTo(upgradeLog)
                    .Build();
            
            var schemaUpgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(
                        Assembly.GetExecutingAssembly(),
                        s => s.StartsWith("DidacticalEnigma.Mem.Database.Migrations",
                            StringComparison.InvariantCulture))
                    .LogTo(upgradeLog)
                    .Build();
            
            var schemaUpgraderResult = schemaUpgrader.PerformUpgrade();
            var sprocUpgradeResult = sprocUpgrader.PerformUpgrade();
            
            return schemaUpgraderResult.Successful && sprocUpgradeResult.Successful;
        }
    }
}