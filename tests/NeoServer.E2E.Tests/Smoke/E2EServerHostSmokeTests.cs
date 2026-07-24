using FluentAssertions;
using NeoServer.E2E.Tests.Client;
using NeoServer.E2E.Tests.Harness;

namespace NeoServer.E2E.Tests.Smoke;

[Collection(E2ECollection.NAME)]
[Trait("Category", "E2E")]
public sealed class E2EServerHostSmokeTests(E2ECollectionFixture fixture)
{
    [SkipOnGitHubActionsFact]
    public async Task Protocol_test_client_connects_to_login_and_game_ports()
    {
        fixture.Host.LoginPort.Should().BeGreaterThan(0);
        fixture.Host.GamePort.Should().BeGreaterThan(0);

        await using var loginClient = new ProtocolTestClient("127.0.0.1", fixture.Host.LoginPort);
        await loginClient.ConnectAsync();
        loginClient.IsConnected.Should().BeTrue();

        await using var gameClient = new ProtocolTestClient("127.0.0.1", fixture.Host.GamePort);
        await gameClient.ConnectAsync();
        gameClient.IsConnected.Should().BeTrue();
    }
}
