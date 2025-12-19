using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

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
                ItemTestDataBuilder.CreateRegularItem(1)
            };

            if (item.Location == new Location((ushort)x, (ushort)y, 7)) items.Add(item);

            world.AddTile(new DynamicTile(new Coordinate(x, y, 7), TileFlag.None, null, [],
                items.ToArray()));
        }

        return new Map(world, new Mock<IEventAggregator>().Object);
    }
}