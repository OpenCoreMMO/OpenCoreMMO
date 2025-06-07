using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
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

        var spectators = target is null
            ? map.GetSpectators(aggressor.Location, true)
            : map.GetSpectators(aggressor.Location, target.Location, true);

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
        if (@event.AttackMissed) SendMissedAttack(@event, connection);

        if (@event.ShootType != default && @event.Target?.Location is not null && !@event.AttackMissed &&
            @event.Target.Location != @event.Aggressor.Location)
        {
            connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(@event.Aggressor.Location,
                @event.Target.Location,
                (byte)@event.ShootType));
        }

        if (@event.Effect != 0 && @event.Target != null)
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

        if (@event.ShootType != default && destLocation != @event.Aggressor.Location)
            connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(@event.Aggressor.Location, destLocation,
                (byte)@event.ShootType));

        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(destLocation, EffectT.Puff));
    }
}