using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items.Services.ItemTransform.Operations;

internal static class ReplaceGroundOperation
{
    public static Result<IItem> Execute(IMap map, IMapService mapService, IItem fromItem, IItem createdItem)
    {
        if (fromItem.Location.Type != LocationType.Ground) return Result<IItem>.NotApplicable;
        if (map[fromItem.Location] is not IDynamicTile) return Result<IItem>.NotApplicable;

        if (fromItem is not Ground) return Result<IItem>.NotApplicable;
        if (createdItem is not Ground createdGround) return Result<IItem>.NotApplicable;

        mapService.ReplaceGround(fromItem.Location, createdGround);
        return Result<IItem>.Ok(createdGround);
    }
}