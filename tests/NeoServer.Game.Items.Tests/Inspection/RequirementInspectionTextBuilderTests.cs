using System.Linq;
using FluentAssertions;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Creatures.Vocation;
using NeoServer.Game.Items.Inspection;
using NeoServer.Game.Tests.Helpers;
using Xunit;

namespace NeoServer.Game.Items.Tests.Inspection;

public class RequirementInspectionTextBuilderTests
{
    [Theory]
    [InlineData("")]
    [InlineData("It can only be wielded properly by knights and paladins.", 1, 2)]
    [InlineData("It can only be wielded properly by knights, paladins and sorcerers.", 1, 2, 3)]
    [InlineData("It can only be wielded properly by knights, paladins, sorcerers and druids.", 1, 2, 3, 4)]
    [InlineData("It can only be wielded properly by knights, sorcerers and druids.", 1, 10, 3, 4)]
    public void Add_HasVocations_ReturnText(string expected, params int[] vocations)
    {
        var vocationStore = new VocationStore();
        vocationStore.Add(1, new Vocation { Name = "Knight" });
        vocationStore.Add(2, new Vocation { Name = "Paladin" });
        vocationStore.Add(3, new Vocation { Name = "Sorcerer" });
        vocationStore.Add(4, new Vocation { Name = "Druid" });

        var input = vocations.Select(x => (byte)x).ToArray();

        var item = ItemTestData.CreateDefenseEquipmentItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, input);

        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("It can only be wielded properly by players of level 10 or higher.", 10)]
    [InlineData("It can only be wielded properly by players of level 1 or higher.", 1)]
    [InlineData("It can only be wielded properly by players of level 200 or higher.", 200)]
    [InlineData("", 0)]
    public void Add_HasLevel_ReturnText(string expected, int level)
    {
        var item = ItemTestData.CreateDefenseEquipmentItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.MinimumLevel, level);

        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("It can only be wielded properly by knights of level 10 or higher.", 10, 1)]
    [InlineData("It can only be wielded properly by knights and paladins of level 1 or higher.", 1, 1, 2)]
    [InlineData("It can only be wielded properly by knights, paladins and sorcerers of level 200 or higher.", 200, 1, 2,
        3)]
    [InlineData("", 0)]
    public void Add_HasLevelAndVocations_ReturnText(string expected, int level, params int[] vocations)
    {
        var vocationStore = new VocationStore();
        vocationStore.Add(1, new Vocation { Name = "Knight" });
        vocationStore.Add(2, new Vocation { Name = "Paladin" });
        vocationStore.Add(3, new Vocation { Name = "Sorcerer" });
        vocationStore.Add(4, new Vocation { Name = "Druid" });

        var input = vocations.Select(x => (byte)x).ToArray();

        var item = ItemTestData.CreateDefenseEquipmentItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.MinimumLevel, level);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, input);

        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Add_HasNoRequirement_ReturnEmpty()
    {
        var item = ItemTestData.CreateCoin(1, 10, 1);
        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineData("It can only be used properly by knights of level 10 or higher.", 10, 1)]
    [InlineData("It can only be used properly by knights and paladins of level 1 or higher.", 1, 1, 2)]
    [InlineData("It can only be used properly by knights, paladins and sorcerers of level 200 or higher.", 200, 1, 2,
        3)]
    [InlineData("", 0)]
    public void Build_UsableHasLevelAndVocations_ReturnText(string expected, int level, params int[] vocations)
    {
        var vocationStore = new VocationStore();
        vocationStore.Add(1, new Vocation { Name = "Knight" });
        vocationStore.Add(2, new Vocation { Name = "Paladin" });
        vocationStore.Add(3, new Vocation { Name = "Sorcerer" });
        vocationStore.Add(4, new Vocation { Name = "Druid" });

        var input = vocations.Select(x => (byte)x).ToArray();

        var item = ItemTestData.CreateAttackRune(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.MinimumLevel, level);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, input);

        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("It can only be consumed properly by knights of level 10 or higher.", 10, 1)]
    [InlineData("It can only be consumed properly by knights and paladins of level 1 or higher.", 1, 1, 2)]
    [InlineData("It can only be consumed properly by knights, paladins and sorcerers of level 200 or higher.", 200, 1,
        2, 3)]
    [InlineData("", 0)]
    public void Build_ConsumableHasLevelAndVocations_ReturnText(string expected, int level, params int[] vocations)
    {
        var vocationStore = new VocationStore();
        vocationStore.Add(1, new Vocation { Name = "Knight" });
        vocationStore.Add(2, new Vocation { Name = "Paladin" });
        vocationStore.Add(3, new Vocation { Name = "Sorcerer" });
        vocationStore.Add(4, new Vocation { Name = "Druid" });

        var input = vocations.Select(x => (byte)x).ToArray();

        var item = ItemTestData.CreatePot(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.MinimumLevel, level);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, input);

        //act
        var actual = RequirementInspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }
}