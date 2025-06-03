using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Game.Tests.Helpers;

namespace NeoServer.Game.World.Tests;

public class MapTest
{
    public Map CreateMap(IItem item)
    {
        var world = new Domain.World.World();

        for (var x = 100; x < 120; x++)
        for (var y = 100; y < 120; y++)
        {
            var items = new List<IItem>
            {
                ItemTestData.CreateRegularItem(1)
            };

            if (item.Location == new Location((ushort)x, (ushort)y, 7)) items.Add(item);

            world.AddTile(new DynamicTile(new Coordinate(x, y, 7), TileFlag.None, null, Array.Empty<IItem>(),
                items.ToArray()));
        }

        return new Map(world);
    }
}