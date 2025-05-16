using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureAttackingEventHandler(IMap map, IGameCreatureManager gameCreatureManager)
    : INetworkingEventHandler<CreatureAttackingEvent>
{
    public void Handle(CreatureAttackingEvent @event)
    {
        var target = @event.Target;
        var aggressor = @event.Aggressor;

        var spectators = target is null ? map.GetSpectators(aggressor.Location, onlyPlayers: true) : map.GetSpectators(aggressor.Location, target.Location, onlyPlayers: true);

        foreach (var spectator in spectators)
        {
            if (spectator is not IPlayer) continue;

            if (!gameCreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            SendAttack(@event, connection);

            connection.Send();
        }
    }

    private static void SendAttack(CreatureAttackingEvent @event,
        IConnection connection)
    {
        if (@event.AttackMissed)
        {
            SendMissedAttack(@event, connection);
        }

        if (@event.ShootType != default && @event.Target?.Location is not null && !@event.AttackMissed)
        {
            connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(@event.Aggressor.Location,
                @event.Target.Location,
                (byte)@event.ShootType));
        }

        if (@event.Effect != EffectT.None && @event.Target != null)
        {
            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Target.Location,
                @event.Effect));
        }

        if (@event.Area?.Length > 0)
        {
            foreach (var location in @event.Area)
            {
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(location, @event.Effect));
            }
        }
    }

    private static void SendMissedAttack(CreatureAttackingEvent @event,
        IConnection connection)
    {
        Location destLocation;
        do
        {
            var index = GameRandom.Random.Next(0, maxValue: @event.Target.Location.Neighbours.Length);
            destLocation = @event.Target.Location.Neighbours[index];
        } while (destLocation == @event.Aggressor.Location);

        if (@event.ShootType != default)
            connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(@event.Aggressor.Location, destLocation,
                (byte)@event.ShootType));
        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(destLocation, EffectT.Puff));
    }
}