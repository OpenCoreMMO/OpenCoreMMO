using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Items.Services.ItemTransform.Operations;

public class ReplaceGroundOperation(ICreatureMovementService creatureMovementService, IMap map)
{
    public Result<IItem> Execute(IItem fromItem, IItem createdItem)
    {
        if (fromItem.Location.Type != LocationType.Ground) return Result<IItem>.NotApplicable;
        if (map[fromItem.Location] is not IDynamicTile) return Result<IItem>.NotApplicable;

        if (fromItem is not IGround) return Result<IItem>.NotApplicable;
        if (createdItem is not IGround createdGround) return Result<IItem>.NotApplicable;

        ReplaceGround(fromItem.Location, createdGround);
        return Result<IItem>.Ok(createdGround);
    }
    
    public void ReplaceGround(Location location, IGround ground)
    {
        if (map[location] is not DynamicTile tile) return;
        tile.ReplaceGround(ground);

        if (!tile.HasHole) return;

        var finalTile = map.GetTileDestination(tile);

        if (finalTile is not DynamicTile toTile) return;

        var removedItems = tile.RemoveAllItems();
        var removedCreatures = tile.RemoveAllCreatures();

        toTile.AddItems(removedItems);

        foreach (var removedCreature in removedCreatures)
            creatureMovementService.MoveCreature(removedCreature, toTile.Location);
    }
}