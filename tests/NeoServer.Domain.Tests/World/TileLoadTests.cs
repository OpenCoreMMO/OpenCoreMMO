using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Events;
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
        
        IDynamicTile TileFunc()
        {
            return MapTestDataBuilder.CreateTile(new Location(100, 100, 7), downItems: food);
        }
        
        var map = MapTestDataBuilder.Build(mockEventAggregator.Object, (Func<IDynamicTile>)TileFunc);
        
        // Act
        food.Reduce();

        // Assert
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<ThingUpdatedOnTileEvent>()),
            Times.AtLeastOnce);
    }
}
