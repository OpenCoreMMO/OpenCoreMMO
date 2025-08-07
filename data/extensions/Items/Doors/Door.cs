using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Extensions.Items.Doors;

public class Door : BaseItem
{
    public Door(IItemType metadata, Location location, IDictionary<ItemTypeAttribute, IConvertible> attributes) :
        base(metadata, location)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        if (Location == usedBy.Location)
        {
            OperationFailService.Send(usedBy.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        if (Map.Instance[Location] is not DynamicTile tile) return;

        var containsLockedOnDescription =
            Metadata.Description?.Contains("locked", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if ((Metadata.Attributes.TryGetCustomAttribute("locked", out bool isLocked) && isLocked) ||
            containsLockedOnDescription)
        {
            OperationFailService.Send(usedBy.CreatureId, TextConstants.IT_IS_LOCKED);
            return;
        }

        // Check if door has transformto attribute
        if (!Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.TransformTo, out var transformToId) || 
            transformToId == 0)
        {
            OperationFailService.Send(usedBy.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        TransformDoor(tile, transformToId);
    }

    private void TransformDoor(DynamicTile dynamicTile, ushort transformToId)
    {
        var wallId = Metadata.Attributes.GetCustomAttribute<ushort>("wall");

        var newDoor = ItemFactory.Instance.Create(transformToId, Location, null);

        dynamicTile.RemoveItem(this, 1, out _);

        if (wallId != default)
        {
            var wall = dynamicTile.TopItems?.ToList()?.FirstOrDefault(x => x.ServerId == wallId);
            if (wall is not null) dynamicTile.RemoveItem(wall, 1, out _);
        }

        dynamicTile.AddItem(newDoor);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.GetAttribute(ItemTypeAttribute.Type) == "door";
    }
}