using System;
using Microsoft.Extensions.Caching.Memory;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Server.Commands.Player.UseItem;

public class HotkeyService
{
    private readonly IMemoryCache _cache;

    public HotkeyService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private void AddToCache(uint playerId, ushort clientId, IContainer container, byte slotIndex)
    {
        _cache.Set((playerId, clientId), new HotkeyItemLocation(container, slotIndex), TimeSpan.FromSeconds(60));
    }

    public IItem GetItem(IPlayer player, ushort clientId)
    {
        if (player is null || clientId == 0) return null;

        var hotKeyItemLocation = _cache.Get((player.Id, clientId)) as HotkeyItemLocation;
        if (hotKeyItemLocation is not null && hotKeyItemLocation.Container.RootParent is IPlayer owner)
        {
            var containerItemId = hotKeyItemLocation.Container.Items.Count > hotKeyItemLocation.SlotIndex
                ? hotKeyItemLocation.Container[hotKeyItemLocation.SlotIndex]?.ClientId
                : null;

            if (owner.Id == player.Id && containerItemId == clientId)
                return hotKeyItemLocation.Container[hotKeyItemLocation.SlotIndex];
        }

        var foundItem = player.Inventory?.BackpackSlot?.GetFirstItemByClientId(clientId); // is not IThing thing) return null;
        if (foundItem?.Item1 is not { } item) return null;
        AddToCache(player.Id, clientId, foundItem.Value.Item2, foundItem.Value.Item3);
        return item;
    }

    private record HotkeyItemLocation(IContainer Container, byte SlotIndex);
}