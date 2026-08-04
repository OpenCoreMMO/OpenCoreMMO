using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Items;

public class Lever : BaseItem
{
    public Lever(IItemType metadata, Location location, IDictionary<ItemTypeAttribute, IConvertible> attributes) : base(
        metadata, location)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        SwitchLever();
    }

    public void SwitchLever()
    {
        var map = IoC.GetInstance<IMap>();
        if (map[Location] is not DynamicTile dynamicTile) return;

        var newLeverId = (ushort)(Metadata.ServerId == 1946 ? 1945 : 1946);
        var newLever = ItemFactory.Instance.Create(newLeverId, Location,
            Metadata.Attributes.ToDictionary<ItemTypeAttribute, IConvertible>());

        dynamicTile.RemoveItem(this, 1, out _);
        dynamicTile.AddItem(newLever);
    }
}