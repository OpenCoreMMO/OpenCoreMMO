using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
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

        if ((Metadata.Attributes.TryGetAttribute("locked", out bool isLocked) && isLocked) ||
            containsLockedOnDescription)
        {
            OperationFailService.Send(usedBy.CreatureId, TextConstants.IT_IS_LOCKED);
            return;
        }

        var mode = Metadata.Attributes.GetAttribute("mode");

        mode = ExtractModeIfEmpty(mode);
        if (mode.Equals("closed", StringComparison.InvariantCultureIgnoreCase))
        {
            OpenDoor(tile);
            return;
        }

        if (mode.Equals("opened", StringComparison.InvariantCultureIgnoreCase))
        {
            CloseDoor(tile);
            return;
        }

        OperationFailService.Send(usedBy.CreatureId, TextConstants.NOT_POSSIBLE);
    }

    private string ExtractModeIfEmpty(string mode)
    {
        if (!string.IsNullOrEmpty(mode)) return mode;

        return Metadata.Name switch
        {
            { } s when s.Contains("closed", StringComparison.InvariantCultureIgnoreCase) => "closed",
            { } s when s.Contains("opened", StringComparison.InvariantCultureIgnoreCase) => "opened",
            _ => mode
        };
    }

    private void OpenDoor(DynamicTile dynamicTile)
    {
        var wallId = Metadata.Attributes.GetAttribute<ushort>("wall");

        if (!Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.TransformTo, out var doorId)) return;

        var door = ItemFactory.Instance.Create(doorId, Location, null);

        dynamicTile.RemoveItem(this, 1, out _);

        if (wallId != default)
        {
            var wall = dynamicTile.TopItems?.ToList()?.FirstOrDefault(x => x.ServerId == wallId);
            if (wall is not null) dynamicTile.RemoveItem(wall, 1, out _);
        }

        dynamicTile.AddItem(door);
    }

    private void CloseDoor(DynamicTile dynamicTile)
    {
        if (!Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.TransformTo, out var doorId)) return;
        var door = ItemFactory.Instance.Create(doorId, Location, null);

        dynamicTile.RemoveItem(this, 1, out _);

        dynamicTile.AddItem(door);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.GetAttribute(ItemTypeAttribute.Type) == "door";
    }
}