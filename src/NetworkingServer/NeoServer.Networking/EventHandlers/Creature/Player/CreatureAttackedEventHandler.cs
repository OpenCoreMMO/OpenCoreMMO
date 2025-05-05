using NeoServer.Game.Combat.Services;
using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Parsers;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class CreatureAttackedEventHandler(IMap map, IGameCreatureManager gameCreatureManager): INetworkingEventHandler<CreatureAttackedEvent>
{
    public void Handle(CreatureAttackedEvent @event)
    {
        var target = @event.AttackInput.Target;
        var aggressor = @event.AttackInput.Aggressor;
        
        var spectators = map.GetSpectators(aggressor.Location, target.Location, onlyPlayers: true);

        foreach (var spectator in spectators)
        {
            if (spectator is not IPlayer) continue;

            if (!gameCreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            SendAttack(@event.AttackInput, connection);

            connection.Send();
        }
    }
    
      private static void SendAttack(AttackInput attackInput,
        IConnection connection)
    {
        // foreach (var attack in attacks)
        // {
            // if (attack.Missed)
            // {
            //     SendMissedAttack(creature, victim, attack, connection);
            // }
            // else
            // {
                if (attackInput.Parameters.ShootType != default && attackInput.Target?.Location is not null)
                    connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(attackInput.Aggressor.Location, attackInput.Target.Location,
                        (byte)attackInput.Parameters.ShootType));

                if (attackInput.Parameters.Effect != EffectT.None)
                {
                    connection.OutgoingPackets.Enqueue(new MagicEffectPacket(attackInput.Target.Location, attackInput.Parameters.Effect));
                }
            // }
            //
            // if (attack.Area?.Any() ?? false)
            //     SpreadAreaEffect(attack, connection);
            //
            // else if (!attack.Missed && victim is not null) SendEffect(attack, connection, victim.Location);
       // }
    }

    private static void SpreadAreaEffect(CombatAttackResult attack, IConnection connection)
    {
        foreach (var coordinate in attack.Area)
        {
            if (coordinate.Missed) continue;
            SendEffect(attack, connection, coordinate.Point.Location);
        }
    }

    private static void SendEffect(CombatAttackResult attack, IConnection connection, Location location)
    {
        attack.EffectT = attack.EffectT == 0 ? EffectT.None : attack.EffectT;
        var effect = attack.EffectT == EffectT.None ? DamageEffectParser.Parse(attack.DamageType) : attack.EffectT;

        if (attack is { EffectT: EffectT.None, DamageType: DamageType.Melee }) return;
        if (effect == EffectT.None) return;

        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(location, effect));
    }

    private static void SendMissedAttack(ICreature creature, ICreature victim, CombatAttackResult attack,
        IConnection connection)
    {
        Location destLocation;
        do
        {
            var index = GameRandom.Random.Next(0, maxValue: victim.Location.Neighbours.Length);
            destLocation = victim.Location.Neighbours[index];
        } while (destLocation == creature.Location);

        if (attack.ShootType != default)
            connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(creature.Location, destLocation,
                (byte)attack.ShootType));
        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(destLocation, EffectT.Puff));
    }
}