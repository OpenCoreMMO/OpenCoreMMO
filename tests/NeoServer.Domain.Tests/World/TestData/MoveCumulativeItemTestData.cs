using System.Collections;
using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Tests.World.TestData;

public class MoveCumulativeItemTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return
        [
            new Data(ItemTestDataBuilder.CreateCumulativeItem(5, 100), 40, new Location(101, 100, 7),
                [], [])
        ];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public class Data
    {
        public Data(ICumulative item, byte amount, Location toLocation, List<IItem> expectedFromTileDowmItems,
            List<IItem> expectedToTileDowmItems)
        {
            Map = CreateMap(item);
            Item = item;
            Amount = amount;
            ToLocation = toLocation;
            ExpectedFromTileDowmItems = expectedFromTileDowmItems;
            ExpectedToTileDowmItems = expectedToTileDowmItems;
        }

        public IMap Map { get; set; }
        public ICumulative Item { get; set; }
        public byte Amount { get; set; }
        public Location ToLocation { get; set; }
        public List<IItem> ExpectedFromTileDowmItems { get; set; }
        public List<IItem> ExpectedToTileDowmItems { get; set; }

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

            return new Map(world, new Mock<IEventAggregator>().Object, new Mock<ILogger>().Object);
        }
    }
}