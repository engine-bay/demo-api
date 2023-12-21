namespace EngineBay.DemoApi.Tests
{
    using EngineBay.Core;
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Xunit;

    public class DbInitialiserTests
    {
        [Fact]
        public async Task CanInitialiseAnInMemoryDb()
        {
            Environment.SetEnvironmentVariable(EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER, DatabaseProviderTypes.InMemory.ToString());
            Environment.SetEnvironmentVariable(EngineBay.DatabaseManagement.EnvironmentVariableConstants.DATABASESEEDDATAPATH, "./TestData");

            var masterDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
               .UseInMemoryDatabase(databaseName: "MasterDb")
               .EnableSensitiveDataLogging()
               .Options;

            var masterDb = new MasterDb(masterDbOptions);

            var masterSqliteDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterSqliteDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterSqliteDb = new MasterSqliteDb(masterSqliteDbOptions);

            var masterSqlServerDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterSqlServerDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterSqlServerDb = new MasterSqlServerDb(masterSqlServerDbOptions);

            var masterPostgresDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterPostgresDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterPostgresDb = new MasterPostgresDb(masterPostgresDbOptions);

            var path = SeedingConfiguration.GetSeedDataPath();

            var logger = Substitute.For<ILogger<DbInitialiser>>();

            var serviceCollection = new ServiceCollection();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var httpContextAccessor = new HttpContextAccessor();

            var initialiser = new DbInitialiser(
                logger,
                masterDb,
                masterSqliteDb,
                masterSqlServerDb,
                masterPostgresDb,
                serviceProvider,
                httpContextAccessor);

            var modules = new List<IModule>();

            modules.Add(new DatabaseManagementModule());

            var exception = Record.Exception(() => initialiser.Run(modules));

            Assert.Null(exception);

            Environment.SetEnvironmentVariable(EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER, DatabaseProviderTypes.SQLite.ToString());
            Environment.SetEnvironmentVariable(EngineBay.DatabaseManagement.EnvironmentVariableConstants.DATABASESEEDDATAPATH, DefaultSeedingConstants.DefaultSeedDataPath);

            await masterDb.DisposeAsync();
            await masterSqliteDb.DisposeAsync();
            await masterSqlServerDb.DisposeAsync();
            await masterPostgresDb.DisposeAsync();
        }

        [Fact]
        public async Task CanNotSeedInvalidDataInAnInMemoryDb()
        {
            Environment.SetEnvironmentVariable(EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER, DatabaseProviderTypes.InMemory.ToString());
            Environment.SetEnvironmentVariable(EngineBay.DatabaseManagement.EnvironmentVariableConstants.DATABASESEEDDATAPATH, "./TestDataBad");

            var masterDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
               .UseInMemoryDatabase(databaseName: "MasterDb")
               .EnableSensitiveDataLogging()
               .Options;

            var masterDb = new MasterDb(masterDbOptions);

            var masterSqliteDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterSqliteDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterSqliteDb = new MasterSqliteDb(masterSqliteDbOptions);

            var masterSqlServerDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterSqlServerDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterSqlServerDb = new MasterSqlServerDb(masterSqlServerDbOptions);

            var masterPostgresDbOptions = new DbContextOptionsBuilder<ModuleWriteDbContext>()
                .UseInMemoryDatabase(databaseName: "MasterPostgresDb")
                .EnableSensitiveDataLogging()
                .Options;

            var masterPostgresDb = new MasterPostgresDb(masterPostgresDbOptions);

            var path = SeedingConfiguration.GetSeedDataPath();

            var logger = Substitute.For<ILogger<DbInitialiser>>();

            var serviceCollection = new ServiceCollection();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var httpContextAccessor = new HttpContextAccessor();

            var initialiser = new DbInitialiser(
                logger,
                masterDb,
                masterSqliteDb,
                masterSqlServerDb,
                masterPostgresDb,
                serviceProvider,
                httpContextAccessor);

            var modules = new List<IModule>();

            modules.Add(new DatabaseManagementModule());

            var exception = Record.Exception(() => initialiser.Run(modules));

            Environment.SetEnvironmentVariable(EngineBay.Persistence.EnvironmentVariableConstants.DATABASEPROVIDER, DatabaseProviderTypes.SQLite.ToString());
            Environment.SetEnvironmentVariable(EngineBay.DatabaseManagement.EnvironmentVariableConstants.DATABASESEEDDATAPATH, DefaultSeedingConstants.DefaultSeedDataPath);

            await masterDb.DisposeAsync();
            await masterSqliteDb.DisposeAsync();
            await masterSqlServerDb.DisposeAsync();
            await masterPostgresDb.DisposeAsync();
        }
    }
}
