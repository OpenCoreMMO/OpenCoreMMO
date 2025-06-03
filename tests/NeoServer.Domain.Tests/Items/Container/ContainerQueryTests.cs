using FluentAssertions;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerQueryTests
{
    [Fact]
    public void Get_First_item_by_client_id_returns_first_item()
    {
        //arrange
        var sut = ItemTestData.CreateContainer();
        var item1 = ItemTestData.CreateWeaponItem(1);
        var item2 = ItemTestData.CreateWeaponItem(2);
        var item3 = ItemTestData.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(item2);
        sut.AddItem(item3);

        //act
        var result = sut.GetFirstItem(1);

        //assert
        result.ItemFound.Should().Be(item1);
        result.Container.Should().Be(sut);
        result.SlotIndex.Should().Be(2);
    }

    [Fact]
    public void Get_First_item_by_client_id_returns_first_item_inside_inner_bag()
    {
        //arrange
        var sut = ItemTestData.CreateContainer();
        var item1 = ItemTestData.CreateWeaponItem(1);

        var innerBag = ItemTestData.CreateContainer();
        var item2 = ItemTestData.CreateWeaponItem(2);
        var item3 = ItemTestData.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(innerBag);
        innerBag.AddItem(item2);
        innerBag.AddItem(item3);

        //act
        var result = sut.GetFirstItem(2);

        //assert
        result.ItemFound.Should().Be(item2);
        result.Container.Should().Be(innerBag);

        result.SlotIndex.Should().Be(1);
    }

    [Fact]
    public void Get_First_item_by_client_id_returns_null_if_not_found()
    {
        //arrange
        var sut = ItemTestData.CreateContainer();
        var item1 = ItemTestData.CreateWeaponItem(1);

        var innerBag = ItemTestData.CreateContainer();
        var item2 = ItemTestData.CreateWeaponItem(2);
        var item3 = ItemTestData.CreateWeaponItem(3);

        sut.AddItem(item1);
        sut.AddItem(innerBag);
        innerBag.AddItem(item2);
        innerBag.AddItem(item3);

        //act
        var result = sut.GetFirstItem(4);

        //assert
        result.ItemFound.Should().Be(null);
        result.Container.Should().Be(null);
        result.SlotIndex.Should().Be(0);
    }
}