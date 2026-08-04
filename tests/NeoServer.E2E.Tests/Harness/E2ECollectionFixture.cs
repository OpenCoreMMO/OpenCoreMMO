namespace NeoServer.E2E.Tests.Harness;

[CollectionDefinition(NAME)]
public sealed class E2ECollection : ICollectionFixture<E2ECollectionFixture>
{
    public const string NAME = "E2E";
}

public sealed class E2ECollectionFixture : IAsyncLifetime
{
    public E2EServerHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Host = await E2EServerHost.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
