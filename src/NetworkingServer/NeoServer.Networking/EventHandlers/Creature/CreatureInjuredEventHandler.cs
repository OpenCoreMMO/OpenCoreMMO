using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Parsers;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureInjuredEventHandler(IMap map, IGameCreatureManager gameCreatureManager) : INetworkingEventHandler<CreatureInjuredEvent>
{
    public void Handle(CreatureInjuredEvent @event)
    {
        var victim = @event.Victim;
        var enemy = @event.Enemy;
        var damages = @event.DamageList;
        
        foreach (var spectator in map.GetPlayersAtPositionZone(victim.Location))
        {
            if (!gameCreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (ReferenceEquals(victim, spectator)) //myself
            {
                connection.OutgoingPackets.Enqueue(new PlayerStatusPacket((IPlayer)victim));

                SendLosePointsMessage(enemy, connection, damages);
            }

            if (ReferenceEquals(enemy, spectator))
                connection.OutgoingPackets.Enqueue(new TextMessagePacket(
                    $"{victim.Name} loses {damages.TotalDamage} due to your attack",
                    TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));

            SendDamageNumbers(damages, victim, connection);

            SendDamageEffect(damages, victim, connection);

            connection.OutgoingPackets.Enqueue(new CreatureHealthPacket(victim));

            connection.Send();
        }
    }

    private static void SendLosePointsMessage(IThing enemy, IConnection connection,
        CombatDamageList damages)
    {
        var totalDamage = damages.TotalDamage;
        var endMessage = enemy is null ? string.Empty : $"due to an attack by a {enemy.Name}";

        if (totalDamage.HealthDamage > 0)
        {
            connection.OutgoingPackets.Enqueue(new TextMessagePacket(
                $"You lose {damages.TotalDamage.HealthDamage} health points {endMessage}",
                TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));
        }

        if (totalDamage.ManaDamage > 0)
        {
            connection.OutgoingPackets.Enqueue(new TextMessagePacket(
                $"You lose {damages.TotalDamage.ManaDamage} mana points {endMessage}",
                TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));
        }
    }

    private static void SendDamageEffect(CombatDamageList damages, ICreature victim, IConnection connection)
    {
        var elementalDamage = damages.ElementalDamage;

        if (elementalDamage.Damage > 0)
        {
            var damageEffect = elementalDamage.Effect == 0
                ? DamageEffectParser.Parse(elementalDamage.Type, victim)
                : elementalDamage.Effect;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(victim.Location, damageEffect));
            return;
        }
        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(victim.Location, EffectT.XBlood));
    }

    private static void SendDamageNumbers(CombatDamageList damages, ICreature victim, IConnection connection)
    {
        foreach (var damage in damages)
        {
            if (damage.Damage <= 0) continue;
            
            var damageTextColor = DamageTextColorParser.Parse(damage.Type, victim);
            connection.OutgoingPackets.Enqueue(new AnimatedTextPacket(victim.Location, damageTextColor,
                damage.Damage.ToString()));
        }
    }
}