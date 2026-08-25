using Xunit;

namespace DareToDance.Api.IntgrationTests.Common;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
