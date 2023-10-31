namespace EngineBay.DemoApi
{
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class MasterPostgresDb : MasterDb
    {
        public MasterPostgresDb(DbContextOptions<ModuleWriteDbContext> options)
            : base(options)
        {
        }

        protected override IReadOnlyCollection<IModuleDbContext> GetRegisteredDbContexts(DbContextOptions<ModuleWriteDbContext> dbOptions)
        {
            return ModuleRegistration.GetRegisteredDbContexts(dbOptions);
        }
    }
}