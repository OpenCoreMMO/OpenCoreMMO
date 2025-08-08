using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.LogIn;

public class DisconnectHandler(IGameCreatureManager gameCreatureManager) : PacketHandler
{
    public override async void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        gameCreatureManager.TryGetCreature(connection.CreatureId, out var creature);

        if (creature is not IPlayer player || player == null || player.IsDead)
            connection.Disconnect();
    }
}