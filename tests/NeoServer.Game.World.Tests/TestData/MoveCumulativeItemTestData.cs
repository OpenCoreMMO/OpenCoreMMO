using System.Collections;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Game.Tests.Helpers;

namespace NeoServer.Game.World.Tests.TestData;

public class MoveCumulativeItemTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new Data(ItemTestData.CreateCumulativeItem(5, 100), 40, new Location(101, 100, 7),
                new List<IItem>(), new List<IItem>())
        };
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
                    ItemTestData.CreateRegularItem(1)
                };

                if (item.Location == new Location((ushort)x, (ushort)y, 7)) items.Add(item);

                world.AddTile(new DynamicTile(new Coordinate(x, y, 7), TileFlag.None, null, new IItem[0],
                    items.ToArray()));
            }

            return new Map(world);
        }
    }
}