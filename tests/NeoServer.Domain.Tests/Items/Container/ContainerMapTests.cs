using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerMapTests
{
    [Fact]
    public void Container_map_returns_all_items_and_their_amount()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateContainer(id: 5);
        var item1 = ItemTestDataBuilder.CreateWeaponItem(1);
        var item2 = ItemTestDataBuilder.CreateAmmo(2, 30);
        var childContainer = ItemTestDataBuilder.CreateContainer(id: 6);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(1);
        var item4 = ItemTestDataBuilder.CreateAmmo(2, 40);

        sut.AddItem(item1);
        sut.AddItem(item2);
        sut.AddItem(childContainer);

        childContainer.AddItem(item3);
        childContainer.AddItem(item4);

        //assert
        sut.Map[1].Should().Be(2);
        sut.Map[2].Should().Be(70);
        sut.Map[6].Should().Be(1);

        sut.Map.Keys.Count.Should().Be(3);
    }
}