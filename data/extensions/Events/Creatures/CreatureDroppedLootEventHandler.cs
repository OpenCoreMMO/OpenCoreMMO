using NeoServer.Extensions.Chat;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Creatures.Monster.Summon;
using NeoServer.Server.Services;

namespace NeoServer.Extensions.Events.Creatures;

public class CreatureDroppedLootEventHandler : IGameEventHandler
{
    public void Execute(ICombatActor deadCreature, ILoot loot)
    {
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