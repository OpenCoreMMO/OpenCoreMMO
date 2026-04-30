using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;

namespace NeoServer.Server.Events.Party;

public class PartyMemberHealedEventHandler : IApplicationEventHandler<CreatureHealedEvent>
{
    public void Handle(CreatureHealedEvent @event)
    {
        var healed = @event.HealedCreature;
        var healer = @event.HealerCreature;
        var amount = @event.Amount;

        if (amount <= 0) return;
        if (healed is not IPlayer healedPlayer) return;
        if (healer is not IPlayer healerPlayer) return;
        if (healedPlayer == healerPlayer) return;
        if (!healerPlayer.PlayerParty.IsInParty) return;

        healerPlayer.PlayerParty.Party.TrackHeal(healerPlayer, healedPlayer, amount);
    }
}
