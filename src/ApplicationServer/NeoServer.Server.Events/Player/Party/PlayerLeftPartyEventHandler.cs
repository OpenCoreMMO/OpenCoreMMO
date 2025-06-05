using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Party;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Party;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player.Party;

public class PlayerLeftPartyEventHandler
{
    private readonly IGameServer game;

    public PlayerLeftPartyEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer oldMember, IParty party)
    {
        if (Guard.AnyNull(oldMember, party)) return;

        game.CreatureManager.GetPlayerConnection(oldMember.CreatureId, out var oldMemberConnection);

        oldMemberConnection?.OutgoingPackets?.Enqueue(new PartyEmblemPacket(oldMember, PartyEmblem.None));
        oldMemberConnection?.OutgoingPackets?.Enqueue(new TextMessagePacket(
            !party.IsOver ? "You have left the party" : TextConstants.PARTY_HAS_BEEN_DISBANDED,
            TextMessageOutgoingType.Description));

        foreach (var member in party.Members)
        {
            if (member == oldMember) continue;

            if (!game.CreatureManager.GetPlayerConnection(member.CreatureId, out var memberConnection)) continue;

            if (!party.IsOver)
                memberConnection.OutgoingPackets.Enqueue(
                    new TextMessagePacket($"{oldMember.Name} has left the party",
                        TextMessageOutgoingType.Description));

            if (member.CanSee(oldMember.Location))
            {
                memberConnection.OutgoingPackets.Enqueue(new PartyEmblemPacket(oldMember, PartyEmblem.None));
                oldMemberConnection?.OutgoingPackets?.Enqueue(new PartyEmblemPacket(member, PartyEmblem.None));
            }

            memberConnection.Send();
        }

        oldMemberConnection?.Send();
    }
}