using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
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

        var itemFound = itemLocation switch
        {
            _ when itemLocation.Type == LocationType.Ground => _gameServer.Map[itemLocation] is not { } tile ? null : tile.TopItemOnStack,
            _ when itemLocation.Type == LocationType.Slot => player.Inventory[Slot.Backpack],
            _ when itemLocation.Type == LocationType.Container => player.Containers[itemLocation.ContainerId][itemLocation.ContainerSlot],
            _ => null
        };

        if (itemFound is not null)
        {
            itemFound.SetNewLocation(itemLocation, force: true);
            return itemFound;
        }

        return itemLocation.Type == LocationType.Slot ? player.Inventory[itemLocation.Slot] : null;
    }
}