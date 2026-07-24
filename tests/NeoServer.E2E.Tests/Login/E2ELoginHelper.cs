using FluentAssertions;
using NeoServer.E2E.Tests.Client;
using NeoServer.E2E.Tests.Client.Protocol;
using NeoServer.E2E.Tests.Harness;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Configurations;
using NeoServer.Server.Standalone.IoC;

namespace NeoServer.E2E.Tests.Login;

internal static class E2ELoginHelper
{
    public static async Task<E2ELoggedInClient> LoginAsync(
        E2EServerHost host,
        CancellationToken cancellationToken = default)
    {
        var serverConfiguration = host.Services.Resolve<ServerConfiguration>();
        var dataPath = serverConfiguration.Data;

        await using var loginClient = new ProtocolTestClient("127.0.0.1", host.LoginPort);
        await loginClient.ConnectAsync(cancellationToken);

        var (accountLoginPacket, loginXteaKey) = LoginPacketBuilder.BuildAccountLogin(
            LoginTestCredentials.Account,
            LoginTestCredentials.Password,
            dataPath);

        await loginClient.SendAsync(accountLoginPacket, cancellationToken);
        loginClient.SetXteaKey(loginXteaKey);

        var characterListPayload = await loginClient.ReceiveEncryptedPacketAsync(
            decryptAtIndexSix: false,
            cancellationToken);

        ProtocolFraming.ReadOpcode(characterListPayload).Should().Be(0x64);

        var characterNames = ProtocolFraming.ParseCharacterNames(characterListPayload);
        characterNames.Should().Contain(LoginTestCredentials.CharacterName);

        var gameClient = new ProtocolTestClient("127.0.0.1", host.GamePort);
        await gameClient.ConnectAsync(cancellationToken);

        var challengePacket = await gameClient.ReceiveLengthPrefixedPacketAsync(cancellationToken);
        var (challengeTimeStamp, challengeNumber) = ProtocolFraming.ParseChallengePacket(challengePacket);

        var (gameLoginPacket, gameXteaKey) = LoginPacketBuilder.BuildGameLogin(
            LoginTestCredentials.Account,
            LoginTestCredentials.Password,
            LoginTestCredentials.CharacterName,
            challengeTimeStamp,
            challengeNumber,
            dataPath);

        await gameClient.SendAsync(gameLoginPacket, cancellationToken);
        gameClient.SetXteaKey(gameXteaKey);

        var enterGamePayload = await gameClient.ReceiveEncryptedPacketAsync(
            decryptAtIndexSix: false,
            cancellationToken);

        ProtocolFraming.ContainsOpcode(enterGamePayload, (byte)GameOutgoingPacketType.SelfAppear).Should().BeTrue();
        ProtocolFraming.ContainsOpcode(enterGamePayload, (byte)GameOutgoingPacketType.MapDescription).Should().BeTrue();

        var loggedInClient = new E2ELoggedInClient(gameClient, gameXteaKey);
        await loggedInClient.DrainPacketsAsync(TimeSpan.FromSeconds(2), cancellationToken);

        return loggedInClient;
    }
}
