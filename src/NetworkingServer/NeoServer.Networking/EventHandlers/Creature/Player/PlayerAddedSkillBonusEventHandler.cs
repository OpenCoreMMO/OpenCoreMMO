using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerAddedSkillBonusEventHandler(IGameServer game) : INetworkingEventHandler<PlayerAddedSkillBonusEvent>
{
    public void Handle(PlayerAddedSkillBonusEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        if (@event.Type == SkillType.Magic)
        {
            connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(@event.Player));
            connection.Send();
            return;
        }

        connection.OutgoingPackets.Enqueue(new PlayerSkillsPacket(@event.Player));
        connection.Send();
    }
}
