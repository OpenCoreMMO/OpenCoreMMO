using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;

namespace NeoServer.Domain.Tests.World;

public class TileLoadTests
{
    [Fact]
    public void Food_when_loaded_from_map_will_fire_map_event()
    {
        //arrange
        var food = ItemTestData.CreateFood(1, 2);

        IDynamicTile TileFunc()
        {
            return MapTestDataBuilder.CreateTile(new Location(100, 100, 7), downItems: food);
        }

        var map = MapTestDataBuilder.Build((Func<IDynamicTile>)TileFunc);

        using var monitor = map.Monitor();

        //act
        food.Reduce();

        //assert
        monitor.Should().Raise(nameof(map.OnThingUpdatedOnTile));
    }
}