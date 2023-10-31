namespace EngineBay.DemoApi
{
    using System;
    using EngineBay.Core;
    using EngineBay.Persistence;

    public class DemoApiModule : BaseModule
    {
        public override IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
        {
            // Register database schema management and initialization
            services.AddTransient<DbInitialiser>();

            // register technology specific services for migrations
            var masterSqliteDbConfiguration = new DatabaseConfiguration<MasterSqliteDb>();
            masterSqliteDbConfiguration.RegisterDatabases(services);

            var masterSqlServerDbConfiguration = new DatabaseConfiguration<MasterSqlServerDb>();
            masterSqlServerDbConfiguration.RegisterDatabases(services);

            var masterPostgresDbConfiguration = new DatabaseConfiguration<MasterPostgresDb>();
            masterPostgresDbConfiguration.RegisterDatabases(services);

            return services;
        }
    }
}
