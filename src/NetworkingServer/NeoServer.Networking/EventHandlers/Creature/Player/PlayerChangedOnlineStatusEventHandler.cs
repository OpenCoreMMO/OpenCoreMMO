using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerChangedOnlineStatusEventHandler(IGameServer game) : INetworkingEventHandler<PlayerChangedOnlineStatusEvent>
{
    public void Handle(PlayerChangedOnlineStatusEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out _)) return;

        foreach (var loggedPlayer in game.CreatureManager.GetAllLoggedPlayers())
        {
            if (!loggedPlayer.Vip.HasInVipList(@event.Player.Id)) continue;
            if (!game.CreatureManager.GetPlayerConnection(loggedPlayer.CreatureId, out var friendConnection))
                continue;

            friendConnection.OutgoingPackets.Enqueue(new PlayerUpdateVipStatusPacket(@event.Player.Id, @event.Online));
            friendConnection.Send();
        }
    }
}
