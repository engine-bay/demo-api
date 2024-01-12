namespace EngineBay.DemoApi.Tests
{
    using Alba;
    using EngineBay.DemoApi;

    public class DemoApiFixture : IDisposable
    {
        public DemoApiFixture()
        {
            Environment.SetEnvironmentVariable("DATABASE_RESET", "true");
            Environment.SetEnvironmentVariable("DATABASE_RESEED", "true");
            Environment.SetEnvironmentVariable("DATABASE_PROVIDER", "InMemory");
            Environment.SetEnvironmentVariable("AUTHENTICATION_METHOD", "Basic");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_ISSUER_SIGNING_KEY", "false");
            Environment.SetEnvironmentVariable("API_DOCUMENTATION_ENABLED", "true");
            Environment.SetEnvironmentVariable("DATABASE_SEED_DATA_PATH", "./test-seed-data");
            this.AlbaHost = Alba.AlbaHost.For<Program>().Result;
        }

        public IAlbaHost AlbaHost { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.AlbaHost.Dispose();
            }
        }
    }
}
