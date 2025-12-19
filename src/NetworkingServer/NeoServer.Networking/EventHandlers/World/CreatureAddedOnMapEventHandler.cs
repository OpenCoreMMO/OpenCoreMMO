using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.World.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.EventHandlers.World;

public class CreatureAddedOnMapEventHandler(IGameServer game) : INetworkingEventHandler<CreatureAddedOnMapEvent>
{
    public void Handle(CreatureAddedOnMapEvent @event)
    {
        Execute(@event.Creature, @event.Cylinder);
    }

    private void Execute(IWalkableCreature creature, ICylinder cylinder)
    {
        if (Guard.AnyNull(cylinder, cylinder.TileSpectators, creature)) return;

        var tile = cylinder.ToTile;
        if (tile.IsNull()) return;

        foreach (var cylinderSpectator in cylinder.TileSpectators)
        {
            var spectator = cylinderSpectator.Spectator;

            if (spectator is not IPlayer spectatorPlayer) continue;
            if (Equals(creature, spectator)) continue;

            if (!spectator.CanSee(creature.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            SendPacketsToSpectator(spectatorPlayer, creature, connection,
                cylinderSpectator.ToStackPosition == byte.MaxValue
                    ? cylinderSpectator.FromStackPosition
                    : cylinderSpectator.ToStackPosition);

            connection.Send();
        }
    }

    private static void SendPacketsToSpectator(IPlayer playerToSend, IWalkableCreature creatureAdded,
        IConnection connection, byte stackPosition)
    {
        connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creatureAdded, stackPosition));
        connection.OutgoingPackets.Enqueue(new AddCreaturePacket(playerToSend, creatureAdded));
        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(creatureAdded.Location, EffectT.BubbleBlue));
    }
}