using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.EventHandlers.Combat;

public class PlayerGainedExperienceEventHandler(IGameServer game)
    : INetworkingEventHandler<CreatureGainedExperienceEvent>
{
    public void Handle(CreatureGainedExperienceEvent @event)
    {
        if (@event is null) return;

        var player = @event.Creature;
        var experience = @event.Experience;
        var experienceText = experience.ToString();

        foreach (var spectator in game.Map.GetPlayersAtPositionZone(player.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new AnimatedTextPacket(player.Location, TextColor.White,
                experienceText));

            TrySendMessageToYourself(player, spectator, connection, experienceText);
            TrySendMessageToSpectator(player, spectator, connection, experienceText);

            connection.Send();
        }
    }

    private static void TrySendMessageToSpectator(ICreature player, ICreature spectator, IConnection connection,
        string experienceText)
    {
        if (Equals(spectator, player)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(
            $"{player.Name} gained {experienceText} experience points.",
            TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));
    }

    private static void TrySendMessageToYourself(ICreature player, ICreature spectator, IConnection connection,
        string experienceText)
    {
        if (!Equals(spectator, player)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(
            $"You gained {experienceText} experience points.",
            TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket((IPlayer)player));
    }
}
