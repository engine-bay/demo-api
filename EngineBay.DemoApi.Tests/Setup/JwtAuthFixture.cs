namespace EngineBay.DemoApi.Tests
{
    using Alba;
    using EngineBay.Authentication;
    using EngineBay.Persistence;

    public class JwtAuthFixture : DemoApiBaseFixture
    {
        public JwtAuthFixture()
        {
            Environment.SetEnvironmentVariable("DATABASE_RESET", "true");
            Environment.SetEnvironmentVariable("DATABASE_RESEED", "true");
            Environment.SetEnvironmentVariable("DATABASE_PROVIDER", "SQLite");
            Environment.SetEnvironmentVariable("DATABASE_CONNECTION_STRING", "Data Source=JwtAuth.sqlite");
            Environment.SetEnvironmentVariable("AUTHENTICATION_METHOD", "JwtBearer");
            Environment.SetEnvironmentVariable("AUTHENTICATION_SECRET", "TestSecret");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_ISSUER_SIGNING_KEY", "false");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_EXPIRY", "false");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_AUDIENCE", "false");
            Environment.SetEnvironmentVariable("AUTHENTICATION_VALIDATE_ISSUER", "false");
            Environment.SetEnvironmentVariable("API_DOCUMENTATION_ENABLED", "true");
            Environment.SetEnvironmentVariable("DATABASE_SEED_DATA_PATH", "./test-seed-data");

            this.AlbaHost = Alba.AlbaHost.For<Program>().Result;

            var username = "TestUser";

            this.SampleUser = this.RegisterUser(username);

            this.SampleUserAuthHeader = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVGVzdFVzZXIifQ.-SswWjf49Cx00qubABuszEQhjfjNMbCb6RogNphTdAM";
        }

        public override string LoginUser(string username, string? password)
        {
            throw new NotImplementedException();
        }

        public override ApplicationUserDto RegisterUser(string username, string? password)
        {
            var user = new CreateUserDto(username);

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
