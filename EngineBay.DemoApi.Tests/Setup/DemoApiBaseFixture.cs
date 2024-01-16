namespace EngineBay.DemoApi.Tests
{
    using Alba;
    using EngineBay.DemoApi;
    using EngineBay.Persistence;

    public abstract class DemoApiBaseFixture : IDisposable
    {
        protected DemoApiBaseFixture()
        {
            this.AlbaHost = Alba.AlbaHost.For<Program>().Result;
            this.SampleUser = new ApplicationUserDto();
            this.SampleUserAuthHeader = string.Empty;
        }

        public IAlbaHost AlbaHost { get; protected set; }

        public ApplicationUserDto SampleUser { get; protected set; }

        public string SampleUserAuthHeader { get; protected set; }

        public abstract ApplicationUserDto RegisterUser(string username, string? password);

        public virtual ApplicationUserDto RegisterUser(string username)
        {
            return this.RegisterUser(username, null);
        }

        public abstract string LoginUser(string username, string? password);

        public virtual string LoginUser(string username)
        {
            return this.LoginUser(username, null);
        }

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
