namespace EngineBay.DemoApi
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using EngineBay.ApiDocumentation;
    using EngineBay.Auditing;
    using EngineBay.Authentication;
    using EngineBay.Core;
    using EngineBay.Cors;
    using EngineBay.DatabaseManagement;
    using EngineBay.DemoModule;
    using EngineBay.Logging;
    using EngineBay.Persistence;
    using EngineBay.RateLimiting;
    using Microsoft.EntityFrameworkCore;

    public static class ModuleRegistration
    {
        public static IServiceCollection RegisterPolicies(this IServiceCollection services)
        {
            foreach (var module in GetRegisteredModules())
            {
                module.RegisterPolicies(services);
            }

            return services;
        }

        public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
        {
            foreach (var module in GetRegisteredModules())
            {
                module.RegisterModule(services, configuration);
            }

            return services;
        }

        public static WebApplication MapModuleEndpoints(this WebApplication app)
        {
            var basePath = "/api";
            var versionNumber = "v1";

            foreach (var module in GetRegisteredModules())
            {
                var routeGroupBuilder = app.MapGroup($"{basePath}/{versionNumber}");

                module.MapEndpoints(routeGroupBuilder);
            }

            return app;
        }

        public static WebApplication AddModuleMiddleware(this WebApplication app)
        {
            foreach (var module in GetRegisteredModules())
            {
                module.AddMiddleware(app);
            }

            return app;
        }

        public static WebApplication InitializeDatabase(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // Seed the database
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var dbInitialiser = serviceProvider.GetRequiredService<DbInitialiser>();

            dbInitialiser.Run(GetRegisteredModules());

            scope.Dispose();

            return app;
        }

        public static IReadOnlyCollection<IModuleDbContext> GetRegisteredDbContexts(IDbContextOptionsFactory dbContextOptionsFactory)
        {
            var dbContexts = new List<IModuleDbContext>();
            foreach (var module in GetRegisteredModules())
            {
                if (module is IDatabaseModule)
                {
                    dbContexts.AddRange(((IDatabaseModule)module).GetRegisteredDbContexts(dbContextOptionsFactory));
                }
            }

            foreach (IModuleDbContext context in dbContexts)
            {
                Console.WriteLine($"Registering DB Context - {context.GetType().Name}");
            }

            return dbContexts;
        }

        private static ReadOnlyCollection<IModule> GetRegisteredModules()
        {
            var modules = new List<IModule>
            {
                new PersistenceModule(),
                new DatabaseManagementModule(),
                new DemoApiModule(),
                new DemoModuleModule(),
                new ApiDocumentationModule(),
                new LoggingModule(),
                new CorsModule(),
                new AuthenticationModule(),
                new AuditingModule(),
                new RateLimitingModule(),
            };

            Console.WriteLine($"Discovered {modules.Count} EngineBay modules");
            return new ReadOnlyCollection<IModule>(modules);
        }
    }
}