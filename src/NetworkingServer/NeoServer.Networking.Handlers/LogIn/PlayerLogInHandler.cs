using System;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using NeoServer.Server.Tasks;
using Serilog.Core;

namespace NeoServer.Networking.Handlers.LogIn;

public class PlayerLogInHandler(IGameServer game, PlayerLogInCommand playerLogInCommand, Logger logger) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (game.State == GameState.Stopped) connection.Close();

        var packet = new PlayerLogInPacket(message);

        var request = new PlayerLogInRequest
        {
            Account = packet.Account,
            Password = packet.Password,
            CharacterName = packet.CharacterName,
            Xtea = packet.Xtea,
            OtcV8Version = (byte)packet.OtcV8Version,
            OperatingSystem = packet.OperatingSystem,
            Version = packet.Version,
            ChallengeTimeStamp = packet.ChallengeTimeStamp,
            ChallengeNumber = packet.ChallengeNumber
        };

        game.Dispatcher.AddEvent(new Event(async void () =>
        {
            try
            {
                var (success, resultMessage) = await playerLogInCommand.Execute(request, connection);
                if (!success) Disconnect(connection, resultMessage);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error processing player log in request: {PacketCharacterName}", packet.CharacterName);
            }
        }));
    }

    private static void Disconnect(IConnection connection, string message)
    {
        connection.Send(new GameServerDisconnectPacket(message));
        connection.Close();
    }
}