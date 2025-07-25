using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Movements.ToInventory;

public sealed class MapToInventoryMovementOperation(IItemMovementService itemMovementService)
{
    public void Execute(
        IPlayer player,
        IMap map,
        ItemThrowPacket itemThrow,
        IScriptManager scriptManager)
    {
        if (map[itemThrow.FromLocation] is not { } fromTile) return;
        if (fromTile.TopDownItemOnStack is not { } item) return;
        if (fromTile is not IDynamicTile dynamicTile) return;

        if (scriptManager.MoveEvents.EquipItem(player, item, itemThrow.ToLocation.Slot, false).HasValue)
            return;

        var result = itemMovementService.Move(player, item, dynamicTile, player.Inventory, itemThrow.Count, 0,
            (byte)itemThrow.ToLocation.Slot);

        if (result.Failed) OperationFailService.Send(player, result.Error);
    }

    public static bool IsApplicable(ItemThrowPacket itemThrowPacket)
    {
        return itemThrowPacket.FromLocation.Type == LocationType.Ground
               && itemThrowPacket.ToLocation.Type == LocationType.Slot;
    }
}