using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Services;

namespace NeoServer.Domain.Creatures.Services;

public class PartyInviteService : IPartyInviteService
{
    private readonly ChatChannelFactory _chatChannelFactory;

    public PartyInviteService(ChatChannelFactory chatChannelFactory)
    {
        _chatChannelFactory = chatChannelFactory;
    }

    public void Invite(IPlayer player, IPlayer invitedPlayer)
    {
        if (invitedPlayer is null || invitedPlayer.CreatureId == player.CreatureId) return;

        if (invitedPlayer.CreatureId == player.CreatureId)
        {
            OperationFailService.Send(player.CreatureId, "You cannot invite yourself.");
            return;
        }

        if (invitedPlayer.PlayerParty.IsInParty)
        {
            OperationFailService.Send(player.CreatureId, $"{invitedPlayer.Name} is already in a party");
            return;
        }

        var partyCreatedNow = player.PlayerParty.Party is null;
        var party = partyCreatedNow ? null : player.PlayerParty.Party;

        if (partyCreatedNow)
        {
            var partyChannel = _chatChannelFactory.CreatePartyChannel();
            party = new Party.Party(player, partyChannel);
        }

        player.PlayerParty.InviteToParty(invitedPlayer, party);
    }
}