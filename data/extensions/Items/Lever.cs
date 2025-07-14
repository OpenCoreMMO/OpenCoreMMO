using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Extensions.Items;

public class Lever : BaseItem
{
    public Lever(IItemType metadata, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(
        metadata, location)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        SwitchLever();
    }

    public void SwitchLever()
    {
        if (Map.Instance[Location] is not DynamicTile dynamicTile) return;

        var newLeverId = (ushort)(Metadata.ServerId == 1946 ? 1945 : 1946);
        var newLever = ItemFactory.Instance.Create(newLeverId, Location,
            Metadata.Attributes.ToDictionary<ItemAttribute, IConvertible>(),
            Metadata.Attributes.ToDictionaryCustom<string, IConvertible>());

        dynamicTile.RemoveItem(this, 1, out _);
        dynamicTile.AddItem(newLever);
    }
}