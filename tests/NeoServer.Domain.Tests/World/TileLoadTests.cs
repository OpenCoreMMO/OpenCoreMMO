using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Map;
using NeoServer.Server.Events.World;

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
        new TileLoadedEventHandler(map).Handle(new TileLoadedEvent(map[100,100,7]));

        // Act
        food.Reduce();

        // Assert - TileChangedEvent should be invoked when food is reduced
        // Note: The static EventAggregator.Invoke is used, so we verify via the mock
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<ThingUpdatedOnTileEvent>()),
            Times.AtLeastOnce);
    }
}
