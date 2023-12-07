namespace EngineBay.DemoApi
{
    using System.Security.Claims;
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

        private readonly IHttpContextAccessor httpContextAccessor;

        public DbInitialiser(
            ILogger<DbInitialiser> logger,
            MasterDb masterDb,
            MasterSqliteDb masterSqliteDb,
            MasterSqlServerDb masterSqlServerDb,
            MasterPostgresDb masterPostgresDb,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.masterDb = masterDb;
            this.masterSqliteDb = masterSqliteDb;
            this.masterSqlServerDb = masterSqlServerDb;
            this.masterPostgresDb = masterPostgresDb;
            this.serviceProvider = serviceProvider;
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Run(IEnumerable<IModule> modules)
        {
            ArgumentNullException.ThrowIfNull(modules);

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

                var systemUser = this.AddSystemUser(databaseProvider);

                this.logger.SeedingDatabase();

                var seedDataPath = SeedingConfiguration.GetSeedDataPath();

                var enumerable = modules as IModule[] ?? modules.ToArray();

                this.LoginAsSystemUser(systemUser);
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

        private void LoginAsSystemUser(SystemUser systemUser)
        {
            var claims = new List<Claim>
                {
                    new Claim(CustomClaimTypes.Name, systemUser.Username),
                    new Claim(CustomClaimTypes.UserId, systemUser.Id.ToString()),
                };
            var identity = new ClaimsIdentity(claims, "SeedAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            this.httpContextAccessor.HttpContext ??= new DefaultHttpContext();
            this.httpContextAccessor.HttpContext.User = claimsPrincipal;
        }

        private SystemUser AddSystemUser(DatabaseProviderTypes databaseProvider)
        {
            var systemUser = new SystemUser();
            switch (databaseProvider)
            {
                case DatabaseProviderTypes.InMemory:
                case DatabaseProviderTypes.SQLite:
                    this.masterSqliteDb.Add(systemUser as ApplicationUser);
                    this.masterSqliteDb.SaveChanges();
                    break;
                case DatabaseProviderTypes.SqlServer:
                    this.masterSqlServerDb.Add(systemUser as ApplicationUser);
                    this.masterSqlServerDb.SaveChanges();
                    break;
                case DatabaseProviderTypes.Postgres:
                    this.masterPostgresDb.Add(systemUser as ApplicationUser);
                    this.masterPostgresDb.SaveChanges();

                    break;
                default:
                    throw new ArgumentException(
                        $"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
            }

            return systemUser;
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
                    throw new ArgumentException(
                        $"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
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
                    throw new ArgumentException(
                        $"Unhandled {EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER} configuration of '{databaseProvider}'.");
            }
        }
    }
}