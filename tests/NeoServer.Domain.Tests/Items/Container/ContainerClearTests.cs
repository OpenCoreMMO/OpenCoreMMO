using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerClearTests
{
    [Fact]
    public void Container_clear_removes_all_items_from_it()
    {
        //arrange
        var bag = ItemTestDataBuilder.CreateContainer();
        var item1 = ItemTestDataBuilder.CreateRegularItem(100);
        var item2 = ItemTestDataBuilder.CreateRegularItem(101);
        var item3 = ItemTestDataBuilder.CreateCumulativeItem(101, 20);
        bag.AddItem(item1);
        bag.AddItem(item2);
        bag.AddItem(item3);

        var innerBag = ItemTestDataBuilder.CreateContainer();
        var item4 = ItemTestDataBuilder.CreateRegularItem(103);
        var item5 = ItemTestDataBuilder.CreateCumulativeItem(104, 100);
        bag.AddItem(innerBag);
        innerBag.AddItem(item4);
        innerBag.AddItem(item5);

        var innerBag2 = ItemTestDataBuilder.CreateContainer();
        var item6 = ItemTestDataBuilder.CreateRegularItem(103);
        var item7 = ItemTestDataBuilder.CreateCumulativeItem(104, 15);
        innerBag.AddItem(innerBag2);
        innerBag2.AddItem(item6);
        innerBag2.AddItem(item7);

        //act
        bag.Clear();

        //assert
        bag.Items.Should().BeNullOrEmpty();
        innerBag.Items.Should().BeNullOrEmpty();
        innerBag2.Items.Should().BeNullOrEmpty();

        bag.Parent.Should().BeNull();
        innerBag.Parent.Should().BeNull();
        innerBag2.Parent.Should().BeNull();

        bag.SlotsUsed.Should().Be(0);
        innerBag.SlotsUsed.Should().Be(0);
        innerBag2.SlotsUsed.Should().Be(0);
    }
}