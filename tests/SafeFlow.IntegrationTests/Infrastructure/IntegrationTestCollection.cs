using Xunit;

namespace SafeFlow.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition that disables parallel execution for integration tests
/// sharing the SQLite in-memory test database instance.
/// </summary>
[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<SafeFlowWebApplicationFactory>
{
}
