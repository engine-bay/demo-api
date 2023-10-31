namespace EngineBay.DemoApi
{
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class MasterSqliteDbFactory : IDesignTimeDbContextFactory<MasterSqliteDb>
    {
        public MasterSqliteDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModuleWriteDbContext>();
            optionsBuilder.UseSqlite(DefaultConnectionStringConstants.DefaultSqliteConnectiontring);

            return new MasterSqliteDb(optionsBuilder.Options);
        }
    }
}