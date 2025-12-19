using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

public class TileLoadTests
{
    [Fact]
    public void Food_when_loaded_from_map_will_fire_map_event()
    {
        // Arrange
        var mockEventAggregator = new Mock<IEventAggregator>();
        var food = ItemTestDataBuilder.CreateFood(1, 2);

        var world = new Domain.World.World();
        var tile = MapTestDataBuilder.CreateTile(new Location(100, 100, 7), downItems: food);
        world.AddTile(tile, new Location(100, 100, 7));
        
        var map = new Map(world, mockEventAggregator.Object);

        // Act
        food.Reduce();

        // Assert
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<ThingUpdatedOnTileEvent>()),
            Times.AtLeastOnce);
    }
}
