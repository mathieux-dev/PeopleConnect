using Xunit;

namespace PeopleConnect.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    // Esta classe não precisa ter membros, ela serve apenas como
    // marcador para o xUnit saber que deve usar o mesmo fixture
    // em todos os testes desta collection
}
