namespace EngineBay.CommunityEdition
{
    using EngineBay.Authentication;
    using EngineBay.Core;
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class DbInitialiser
    {
        private readonly ILogger<DbInitialiser> logger;
        private readonly MasterDb masterDb;
        private readonly MasterSqliteDb masterSqliteDb;
        private readonly MasterSqlServerDb masterSqlServerDb;

        private readonly MasterPostgresDb masterPostgresDb;

        private readonly IServiceProvider serviceProvider;

        public DbInitialiser(
            ILogger<DbInitialiser> logger,
            MasterDb masterDb,
            MasterSqliteDb masterSqliteDb,
            MasterSqlServerDb masterSqlServerDb,
            MasterPostgresDb masterPostgresDb,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.masterDb = masterDb;
            this.masterSqliteDb = masterSqliteDb;
            this.masterSqlServerDb = masterSqlServerDb;
            this.masterPostgresDb = masterPostgresDb;
            this.serviceProvider = serviceProvider;
        }

        public void Run(IEnumerable<IModule> modules)
        {
            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            this.logger.InitializingDatabase();

            var databaseProvider = BaseDatabaseConfiguration.GetDatabaseProvider();
            var shouldResetDatabase = BaseDatabaseConfiguration.IsDatabaseReset();
            var shouldReseedDatabase = BaseDatabaseConfiguration.IsDatabaseReseeded();
            var shouldExitAfterMigrations = BaseDatabaseConfiguration.ShouldExitAfterMigrations();
            var shouldExitAfterSeeding = BaseDatabaseConfiguration.ShouldExitAfterSeeding();

            if (shouldResetDatabase)
            {
                this.DeleteDatabase(databaseProvider);
            }

            this.ApplyMigrations(databaseProvider);

            if (shouldExitAfterMigrations)
            {
                this.logger.ExitingProcess();

                Environment.Exit(0);
            }

            if (shouldResetDatabase || shouldReseedDatabase)
            {
                this.logger.CreatingRootSystemUser();

                this.AddSystemUser(databaseProvider);

                this.logger.SeedingDatabase();

                var seedDataPath = SeedingConfiguration.GetSeedDataPath();

                var enumerable = modules as IModule[] ?? modules.ToArray();

                foreach (var module in enumerable)
                {
                    module.SeedDatabase(seedDataPath, this.serviceProvider);
                }

                if (shouldExitAfterSeeding)
                {
                    this.logger.ExitingProcess();

                    Environment.Exit(0);
                }
            }
        }

        private void AddSystemUser(DatabaseProviderTypes databaseProvider)
        {
            var systemUser = new SystemUser();
            switch (databaseProvider)
            {
                case DatabaseProviderTypes.InMemory:
                case DatabaseProviderTypes.SQLite:
                    this.masterSqliteDb.Add(systemUser as ApplicationUser);
                    this.masterSqliteDb.SaveChanges(systemUser);
                    break;
                case DatabaseProviderTypes.SqlServer:
                    this.masterSqlServerDb.Add(systemUser as ApplicationUser);
                    this.masterSqlServerDb.SaveChanges(systemUser);
                    break;
                case DatabaseProviderTypes.Postgres:
                    this.masterPostgresDb.Add(systemUser as ApplicationUser);
                    this.masterPostgresDb.SaveChanges(systemUser);

                    break;
                default:
                    throw new ArgumentException($"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
            }
        }

        private void ApplyMigrations(DatabaseProviderTypes databaseProvider)
        {
            this.logger.ApplyingDatabaseMigrations(databaseProvider);

            switch (databaseProvider)
            {
                case DatabaseProviderTypes.InMemory:
                case DatabaseProviderTypes.SQLite:
                    if (this.masterSqliteDb.Database.IsRelational())
                    {
                        this.masterSqliteDb.Database.Migrate();
                    }

                    break;
                case DatabaseProviderTypes.SqlServer:
                    if (this.masterSqlServerDb.Database.IsRelational())
                    {
                        this.masterSqlServerDb.Database.Migrate();
                    }

                    break;
                case DatabaseProviderTypes.Postgres:
                    if (this.masterPostgresDb.Database.IsRelational())
                    {
                        this.masterPostgresDb.Database.Migrate();
                    }

                    break;
                default:
                    throw new ArgumentException($"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
            }

            this.logger.DatabaseMigrationsComplete();
        }

        private void DeleteDatabase(DatabaseProviderTypes databaseProvider)
        {
            // For development and testing we want to be able to delete and recreate each time on startup for a deterministic state.
            this.logger.DeletingDatabase();

            switch (databaseProvider)
            {
                case DatabaseProviderTypes.InMemory:
                case DatabaseProviderTypes.SQLite:
                    this.masterSqliteDb.Database.EnsureDeleted();
                    break;
                case DatabaseProviderTypes.SqlServer:
                    this.masterSqlServerDb.Database.EnsureDeleted();
                    break;
                case DatabaseProviderTypes.Postgres:
                    this.masterPostgresDb.Database.EnsureDeleted();
                    break;
                default:
                    throw new ArgumentException($"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
            }
        }
    }
}