namespace EngineBay.DemoApi
{
    using System.Collections.Generic;
    using System.Linq;
    using EngineBay.ApiDocumentation;
    using EngineBay.Authentication;
    using EngineBay.Core;
    using EngineBay.Cors;
    using EngineBay.DatabaseManagement;
    using EngineBay.DemoModule;
    using EngineBay.Logging;
    using EngineBay.Persistence;

    public static class ModuleRegistration
    {
        private static readonly List<IModule> RegisteredModules = new List<IModule>();

        public static IServiceCollection RegisterPolicies(this IServiceCollection services)
        {
            foreach (var module in RegisteredModules)
            {
                module.RegisterPolicies(services);
            }

            return services;
        }

        public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
        {
            var modules = GetRegisteredModules();
            foreach (var module in modules)
            {
                module.RegisterModule(services, configuration);
                RegisteredModules.Add(module);
            }

            return services;
        }

        public static WebApplication MapModuleEndpoints(this WebApplication app)
        {
            var basePath = "/api";
            var versionNumber = "v1";

            foreach (var module in RegisteredModules)
            {
                var routeGroupBuilder = app.MapGroup($"{basePath}/{versionNumber}");

                module.MapEndpoints(routeGroupBuilder);
            }

            return app;
        }

        public static WebApplication AddModuleMiddleware(this WebApplication app)
        {
            foreach (var module in RegisteredModules)
            {
                module.AddMiddleware(app);
            }

            return app;
        }

        public static WebApplication InitializeDatabase(this WebApplication app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Seed the database
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var dbInitialiser = serviceProvider.GetRequiredService<DbInitialiser>();

            dbInitialiser.Run(RegisteredModules);

            scope.Dispose();

            return app;
        }

        private static IEnumerable<IModule> GetRegisteredModules()
        {
            var modules = new List<IModule>
            {
                new PersistenceModule(),
                new DatabaseManagementModule(),
                new DemoModuleModule(),
                new ApiDocumentationModule(),
                new LoggingModule(),
                new CorsModule(),
                new AuthenticationModule(),
                new DemoModuleModule(),
            };

            Console.WriteLine($"Discovered {modules.Count} EngineBay modules");
            return modules;
        }
    }
}