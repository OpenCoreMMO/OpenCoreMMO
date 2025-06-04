using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Talks;

public class CreatureHearEventHandler
{
    private readonly IGameServer game;

    public CreatureHearEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(ICreature from, ISociableCreature receiver, SpeechType type, string message)
    {
        if (from is null || receiver is null || type == SpeechType.None || string.IsNullOrEmpty(message)) return;

        SendMessage(from, receiver, type, message);
    }

    private void SendMessage(ICreature from, ICreature to, SpeechType type, string message)
    {
        if (!game.CreatureManager.GetPlayerConnection(to.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new CreatureSayPacket(from, type, message));

        connection.Send();
    }
}