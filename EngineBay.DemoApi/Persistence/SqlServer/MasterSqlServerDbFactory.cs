namespace EngineBay.DemoApi
{
    using EngineBay.DatabaseManagement;
    using EngineBay.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class MasterSqlServerDbFactory : IDesignTimeDbContextFactory<MasterSqlServerDb>
    {
        public MasterSqlServerDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModuleWriteDbContext>();
            optionsBuilder.UseSqlServer();

            return new MasterSqlServerDb(optionsBuilder.Options);
        }
    }
}