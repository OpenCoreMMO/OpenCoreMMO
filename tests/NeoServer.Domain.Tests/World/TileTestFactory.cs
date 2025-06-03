using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

public class TileTestFactory
{
    private static ITile CreateTile(Coordinate coord, params IItem[] item)
    {
        var topItems = new List<IItem>
        {
            ItemTestData.CreateTopItem(1, 1),
            ItemTestData.CreateTopItem(2, 2)
        };

        var items = new List<IItem>
        {
            ItemTestData.CreateRegularItem(100),
            ItemTestData.CreateRegularItem(200)
        };
        items.AddRange(item);

        var tile = new DynamicTile(coord, TileFlag.None, null, topItems.ToArray(), items.ToArray());
        return tile;
    }
}