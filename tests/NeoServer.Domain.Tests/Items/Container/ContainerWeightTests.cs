using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerWeightTests
{
    [Fact]
    public void Container_weight_increases_when_items_are_added_to_it()
    {
        //arrange
        var backpack = ItemTestDataBuilder.CreateBackpack(weight: 18);

        var weapon = ItemTestDataBuilder.CreateWeaponItem(2, weight: 40);
        var fiveArrows = ItemTestDataBuilder.CreateCumulativeItem(3, 5, weight: 0.70f);
        var twoArrows = ItemTestDataBuilder.CreateCumulativeItem(3, 2, weight: 0.70f);

        //assert
        backpack.Weight.Should().Be(18);

        //act
        backpack.AddItem(weapon);

        //assert
        backpack.Weight.Should().Be(58);

        //act
        backpack.AddItem(fiveArrows);

        //assert
        backpack.Weight.Should().Be(61.5f);

        //act
        backpack.AddItem(twoArrows);

        //assert
        backpack.Weight.Should().Be(62.9f);
    }

    [Fact]
    public void Container_weight_decreases_when_items_are_removed_from_it()
    {
        //arrange

        var weapon = ItemTestDataBuilder.CreateWeaponItem(2, weight: 40);
        var twoArrows = ItemTestDataBuilder.CreateCumulativeItem(3, 2, weight: 0.70f);
        var twoMeat = ItemTestDataBuilder.CreateFood(4, 2);

        var backpack = ItemTestDataBuilder.CreateContainer(weight: 8, children: new List<IItem>
        {
            twoArrows, weapon, twoMeat
        });

        //assert
        backpack.Weight.Should().Be(75.40f);

        //act
        backpack.RemoveItem(weapon, 1);

        //assert
        backpack.Weight.Should().Be(35.4f);

        //act
        backpack.RemoveItem(twoArrows, 1);

        //assert
        backpack.Weight.Should().Be(34.7f);

        //act
        backpack.RemoveItem(twoArrows, 1);

        //assert
        backpack.Weight.Should().Be(34f);

        //act
        twoMeat.Reduce();

        //assert
        backpack.Weight.Should().Be(21f);

        //act
        twoMeat.Reduce();

        //assert
        backpack.Weight.Should().Be(8f);
    }
}