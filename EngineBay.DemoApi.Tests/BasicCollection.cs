namespace EngineBay.DemoApi.Tests
{
    using Xunit;

    [CollectionDefinition(BASICCOLLECTION)]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class BasicCollection : ICollectionFixture<DemoApiFixture>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public const string BASICCOLLECTION = "Basic auth in memory instance";
    }
}
