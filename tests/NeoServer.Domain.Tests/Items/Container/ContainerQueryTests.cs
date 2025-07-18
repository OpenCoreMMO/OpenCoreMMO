using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerQueryTests
{
    [Fact]
    public void Get_First_item_by_client_id_returns_first_item()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateContainer();
        var item1 = ItemTestDataBuilder.CreateWeaponItem(1);
        var item2 = ItemTestDataBuilder.CreateWeaponItem(2);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(item2);
        sut.AddItem(item3);

        //act
        var result = sut.GetFirstItemByClientId(1);

        //assert
        result.ItemFound.Should().Be(item1);
        result.Container.Should().Be(sut);
        result.SlotIndex.Should().Be(2);
    }

    [Fact]
    public void Get_First_item_by_client_id_returns_first_item_inside_inner_bag()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateContainer();
        var item1 = ItemTestDataBuilder.CreateWeaponItem(1);

        var innerBag = ItemTestDataBuilder.CreateContainer();
        var item2 = ItemTestDataBuilder.CreateWeaponItem(2);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(innerBag);
        innerBag.AddItem(item2);
        innerBag.AddItem(item3);

        //act
        var result = sut.GetFirstItemByClientId(2);

        //assert
        result.ItemFound.Should().Be(item2);
        result.Container.Should().Be(innerBag);

        result.SlotIndex.Should().Be(1);
    }

    [Fact]
    public void Get_First_item_by_client_id_returns_null_if_not_found()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateContainer();
        var item1 = ItemTestDataBuilder.CreateWeaponItem(1);

        var innerBag = ItemTestDataBuilder.CreateContainer();
        var item2 = ItemTestDataBuilder.CreateWeaponItem(2);
        var item3 = ItemTestDataBuilder.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(innerBag);
        innerBag.AddItem(item2);
        innerBag.AddItem(item3);

        //act
        var result = sut.GetFirstItemByClientId(4);

        //assert
        result.ItemFound.Should().Be(null);
        result.Container.Should().Be(null);
        result.SlotIndex.Should().Be(0);
    }
}