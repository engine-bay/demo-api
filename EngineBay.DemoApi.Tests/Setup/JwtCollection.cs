namespace EngineBay.DemoApi.Tests
{
    using Xunit;

    [CollectionDefinition(JWTCOLLECTION, DisableParallelization = true)]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class JwtCollection : ICollectionFixture<JwtAuthFixture>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public const string JWTCOLLECTION = "Jwt auth in memory instance";
    }
}
