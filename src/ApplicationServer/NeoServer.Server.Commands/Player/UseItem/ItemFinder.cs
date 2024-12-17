using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Commands.Player.UseItem;

public class ItemFinderService
{
    private readonly IGameServer _gameServer;
    private readonly HotkeyService _hotkeyService;

    public ItemFinderService(HotkeyService hotkeyService, IGameServer gameServer)
    {
        _hotkeyService = hotkeyService;
        _gameServer = gameServer;
    }

    public IItem Find(IPlayer player, Location itemLocation, ushort clientId)
    {
        if (itemLocation.IsHotkey) return _hotkeyService.GetItem(player, clientId);

        if (itemLocation.Type == LocationType.Ground)
            return _gameServer.Map[itemLocation] is not { } tile ? null : tile.TopItemOnStack;

        if (itemLocation.Slot == Slot.Backpack)
        {
            var item = player.Inventory[Slot.Backpack];
            item.SetNewLocation(itemLocation);
            return item;
        }

        if (itemLocation.Type == LocationType.Container)
        {
            var item = player.Containers[itemLocation.ContainerId][itemLocation.ContainerSlot];
            item.SetNewLocation(itemLocation);
            return item;
        }

        if (itemLocation.Type == LocationType.Slot) return player.Inventory[itemLocation.Slot];

        return null;
    }
}