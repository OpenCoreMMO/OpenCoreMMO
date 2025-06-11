using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Chat.Factory;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Services;

public class PartyInviteServiceTest
{
    [Fact]
    public void Invite_CreatesParty_WhenNeitherPlayerAreInAParty()
    {
        var partyLeader = PlayerTestDataBuilder.Build();
        Assert.False(partyLeader.PlayerParty.IsInParty);

        var invitedPlayer = PlayerTestDataBuilder.Build(2);
        Assert.False(invitedPlayer.PlayerParty.IsInParty);

        var chatChannelFactory = new ChatChannelFactory(
            new List<IChatChannelEventSubscriber>(),
            new ChatChannelStore(),
            null
        );
        var partyInviteService = new PartyInviteService(chatChannelFactory);

        partyInviteService.Invite(partyLeader, invitedPlayer);

        Assert.True(partyLeader.PlayerParty.IsInParty); // party leader has created a party by inviting someone.
        Assert.False(invitedPlayer.PlayerParty
            .IsInParty); // invited player has not yet accepted party invitation, therefore they are not in a party.
        Assert.True(
            partyLeader.PlayerParty.Party.IsInvited(invitedPlayer)); // invited player should be listed as invited.
        Assert.Single(partyLeader.PlayerParty.Party
            .Members); // party leader should be added to the list of members upon creation.
        Assert.Equal(partyLeader,
            partyLeader.PlayerParty.Party.Members.First()); // The party leader should be the only member at this point.
        Assert.True(
            partyLeader.PlayerParty.Party.IsLeader(partyLeader)); // The party leader should be the party leader.
    }
}