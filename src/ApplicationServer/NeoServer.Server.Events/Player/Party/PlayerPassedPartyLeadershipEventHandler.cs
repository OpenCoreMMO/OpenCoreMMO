using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Party;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Party;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player.Party;

public class PlayerPassedPartyLeadershipEventHandler
{
    private readonly IGameServer game;

    public PlayerPassedPartyLeadershipEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer oldLeader, IPlayer newLeader, IParty party)
    {
        if (Guard.AnyNull(oldLeader, newLeader, party)) return;

        foreach (var member in party.Members)
        {
            if (!game.CreatureManager.GetPlayerConnection(member.CreatureId, out var connection)) continue;

            if (member == newLeader)
                connection.OutgoingPackets.Enqueue(new TextMessagePacket("You are now the leader of the party.",
                    TextMessageOutgoingType.Description));
            else
                connection.OutgoingPackets.Enqueue(new TextMessagePacket(
                    $"{newLeader.Name} is now the leader of the party", TextMessageOutgoingType.Description));

            connection.OutgoingPackets.Enqueue(new PartyEmblemPacket(newLeader, PartyEmblem.Leader));
            connection.OutgoingPackets.Enqueue(new PartyEmblemPacket(oldLeader, PartyEmblem.Member));
            connection.Send();
        }

        foreach (var inviteId in party.Invites)
        {
            if (!game.CreatureManager.GetPlayerConnection(inviteId, out var connection)) continue;
            if (!game.CreatureManager.TryGetPlayer(inviteId, out _)) continue;

            connection.OutgoingPackets.Enqueue(new PartyEmblemPacket(newLeader, PartyEmblem.LeaderInvited));
            connection.OutgoingPackets.Enqueue(new PartyEmblemPacket(oldLeader, PartyEmblem.None));
            connection.Send();
        }
    }
}