namespace EngineBay.DemoApi
{
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;

    public class MasterSqliteDb : MasterDb
    {
        public MasterSqliteDb(DbContextOptions<ModuleWriteDbContext> options)
            : base(options)
        {
        }

        protected override IReadOnlyCollection<IModuleDbContext> GetRegisteredDbContexts(IDbContextOptionsFactory dbContextOptionsFactory)
        {
            return ModuleRegistration.GetRegisteredDbContexts(dbContextOptionsFactory);
        }
    }
}