using Xunit;

namespace DareToDance.Api.IntgrationTests.Common;

// Sve integracione test klase dele JEDNU instancu CustomWebApplicationFactory (i time jedan
// Postgres Testcontainers kontejner) umesto da svaka pravi svoj. xUnit garantuje da se testovi
// unutar iste kolekcije izvrsavaju sekvencijalno (nikad paralelno sa drugim testom iz iste
// kolekcije), pa deljena baza ne izaziva trke izmedju testova.
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
