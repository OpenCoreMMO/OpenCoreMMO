using FluentAssertions;
using NeoServer.E2E.Tests.Harness;
using NeoServer.Networking.Packets.Outgoing;

namespace NeoServer.E2E.Tests.Login;

[Collection(E2ECollection.NAME)]
[Trait("Category", "E2E")]
public sealed class PlayerLoginTests(E2ECollectionFixture fixture)
{
    [SkipOnGitHubActionsFact]
    public async Task Player_receives_character_list_and_enters_game_after_login()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var loggedInClient = await E2ELoginHelper.LoginAsync(fixture.Host, timeout.Token);
        loggedInClient.Client.IsConnected.Should().BeTrue();
    }
}
