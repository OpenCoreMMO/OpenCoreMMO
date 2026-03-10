using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Tests.Helpers.Map;

public static class MapTestDataBuilder
{
    public static IMap Build(params ITile[] tiles)
    {
        var world = new Domain.World.World();
        var map = new Domain.World.Map.Map(world, new Mock<IEventAggregator>().Object, new Mock<ILogger>().Object);

        foreach (var tile in tiles) world.AddTile(tile);

        return map;
    }

    public static IMap Build(IEventAggregator eventAggregator, params Func<ITile>[] tiles)
    {
        var world = new Domain.World.World();
        var map = new Domain.World.Map.Map(world, eventAggregator, new Mock<ILogger>().Object);

        foreach (var tile in tiles) world.AddTile(tile?.Invoke());

        return map;
    }

    public static IMap Build(int fromX, int toX, int fromY, int toY, int fromZ, int toZ, bool addGround = true,
        IDictionary<Location, IItem[]> topItems = null,
        List<Location> staticTiles = null)
    {
        topItems ??= new Dictionary<Location, IItem[]>();
        staticTiles ??= [];

        var world = new Domain.World.World();
        var map = new Domain.World.Map.Map(world, new Mock<IEventAggregator>().Object,  new Mock<ILogger>().Object);

        for (var x = fromX; x <= toX; x++)
        for (var y = fromY; y <= toY; y++)
        for (var z = fromZ; z <= toZ; z++)
        {
            IGround ground = null;

            var location = new Location((ushort)x, (ushort)y, (byte)z);

            if (addGround)
                ground = new Ground(new ItemType().SetClientId(1), new Location((ushort)x, (ushort)y, (byte)z));

            topItems.TryGetValue(location, out var items);

            if (staticTiles.Contains(location)) world.AddTile(new StaticTile(new Coordinate(x, y, (sbyte)z)));

            world.AddTile(new DynamicTile(new Coordinate(x, y, (sbyte)z), TileFlag.None, ground,
                items ?? [], null), location);
        }

        return map;
    }

    public static Ground CreateGround(Location location, ushort id = 1, int speed = 50)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.Attributes?.SetAttribute(ItemTypeAttribute.Speed, speed);

        return new Ground(itemType, location);
    }

    public static IDynamicTile CreateTile(Location location, ushort id = 1, int speed = 50)
    {
        var random = new Random();
        var ground = CreateGround(location, (ushort)random.Next(1, ushort.MaxValue));

        return new DynamicTile(new Coordinate(location), TileFlag.None, ground, [],
            []);
    }

    public static IDynamicTile CreateTile(Location location, ushort id = 1, int speed = 50, params IItem[] downItems)
    {
        var random = new Random();
        var ground = CreateGround(location, (ushort)random.Next(1, ushort.MaxValue));

        return new DynamicTile(new Coordinate(location), TileFlag.None, ground, [],
            downItems);
    }
}