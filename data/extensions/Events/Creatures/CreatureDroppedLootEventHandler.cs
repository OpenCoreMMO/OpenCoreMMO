using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Extensions.Chat;
using NeoServer.Server.Services;

namespace NeoServer.Extensions.Events.Creatures;

public class CreatureDroppedLootEventHandler : IApplicationEventHandler<CreatureDroppedLootEvent>
{
    public void Handle(CreatureDroppedLootEvent @event)
    {
        if (@event is null) return;

        var deadCreature = @event.Actor;
        var loot = @event.Loot;

        if (deadCreature is Summon) return;
        if (loot?.Owners is null) return;

        foreach (var owner in loot.Owners)
        {
            if (owner is not IPlayer player) continue;

            if (player.Channels.PersonalChannels is null) continue;

            var lootContentText = deadCreature?.Corpse?.ToString() ?? "nothing";

            var lootText = $"Loot of a {deadCreature?.Name.ToLower()}: {lootContentText}.";

            SendToLootChannel(player, lootText);
            NotificationSenderService.Send(player, lootText);
        }
    }

    private static void SendToLootChannel(IPlayer player, string lootText)
    {
        foreach (var channel in player.Channels.PersonalChannels)
        {
            if (channel is not LootChannel lootChannel) continue;

            lootChannel.WriteMessage(lootText,
                out _);
        }
    }
}
