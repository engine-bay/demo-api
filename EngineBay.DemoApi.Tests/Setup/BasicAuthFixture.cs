namespace EngineBay.DemoApi.Tests
{
    using Alba;
    using EngineBay.Authentication;
    using EngineBay.Persistence;

    public class BasicAuthFixture : DemoApiBaseFixture
    {
        public BasicAuthFixture()
        {
            Environment.SetEnvironmentVariable("DATABASE_RESET", "true");
            Environment.SetEnvironmentVariable("DATABASE_RESEED", "true");

            // Environment.SetEnvironmentVariable("DATABASE_PROVIDER", "InMemory");
            Environment.SetEnvironmentVariable("DATABASE_PROVIDER", "SQLite");

            // Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Data Source=:memory:");

            // Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Data Source=BasicAuth;Mode=Memory");
            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Data Source=BasicAuth.sqlite");
            Environment.SetEnvironmentVariable("AUTHENTICATION_METHOD", "Basic");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_ISSUER_SIGNING_KEY", "false");
            Environment.SetEnvironmentVariable("API_DOCUMENTATION_ENABLED", "true");
            Environment.SetEnvironmentVariable("DATABASE_SEED_DATA_PATH", "./test-seed-data");

            this.AlbaHost = Alba.AlbaHost.For<Program>().Result;

            var username = "TestUser";
            var password = "StrongPassword";

            this.SampleUser = this.RegisterUser(username, password);

            this.SampleUserAuthHeader = this.LoginUser(username, password);
        }

        public override string LoginUser(string username, string? password)
        {
            return "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
        }

        public override ApplicationUserDto RegisterUser(string username, string? password)
        {
            var user = new CreateBasicAuthUserDto()
            {
                Username = username,
                Password = password,
            };

            var registerResult = this.AlbaHost.Scenario(scenario =>
            {
                scenario.Post
                .Json(user)
                .ToUrl("/api/v1/register");
                scenario.StatusCodeShouldBeOk();
            }).Result;

            var registerOutput = registerResult.ReadAsJson<ApplicationUserDto>();
            ArgumentNullException.ThrowIfNull(registerOutput);
            return registerOutput;
        }
    }
}
