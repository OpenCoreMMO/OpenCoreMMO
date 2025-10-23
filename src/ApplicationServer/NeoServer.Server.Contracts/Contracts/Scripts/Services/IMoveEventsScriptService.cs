using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IMoveEventsScriptService
{
    void ItemMove(IItem item, ITile tile, bool isAdd);
    bool? EquipItem(IPlayer player, IItem item, Slot slot, bool isChecks);
    bool? DeEquipItem(IPlayer player, IItem item, Slot slot, bool isChecks);
}