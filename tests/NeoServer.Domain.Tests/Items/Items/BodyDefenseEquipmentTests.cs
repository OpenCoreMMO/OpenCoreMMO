using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items;

public class BodyDefenseEquipmentTests
{
    [Fact]
    public void InspectionText_Armor_ReturnsText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Armor, 5)
            ]);

        //assert
        sut.InspectionText.Should().Be("(Arm: 5)");
    }

    [Fact]
    public void InspectionText_Shield_ReturnsText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Defense, 50)
            ]);

        //assert
        sut.InspectionText.Should().Be("(Def: 50)");
    }

    [Fact]
    public void Pickupable_ReturnsTrue()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Defense, 50)
            ]);

        //assert
        sut.Pickupable.Should().BeTrue();
    }

    [Fact]
    public void IsApplicable_Null_ReturnsFalse()
    {
        //act
        var actual = BodyDefenseEquipment.IsApplicable(null);

        //assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("body")]
    [InlineData("legs")]
    [InlineData("head")]
    [InlineData("feet")]
    [InlineData("shield")]
    [InlineData("ring")]
    [InlineData("necklace")]
    public void IsApplicable_ReturnsTrue(string slot)
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, slot)
            ]);

        //act
        var actual = BodyDefenseEquipment.IsApplicable(sut.Metadata);

        //assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData("backpack")]
    [InlineData("ammo")]
    [InlineData("two-handed")]
    [InlineData("weapon")]
    public void IsApplicable_ReturnsFalse(string slot)
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, slot)
            ]);
        //act
        var actual = BodyDefenseEquipment.IsApplicable(sut.Metadata);

        //assert
        actual.Should().BeFalse();
    }

    #region CanBeDressed Tests

    [InlineData(2, 9, 1, 10)]
    [InlineData(2, 9, 2, 10)]
    [InlineData(2, 10, 1, 10)]
    [Theory]
    public void CanBeDressed_PlayerHasNeitherLevelNorVocation_ReturnsFalse(int playerVocation, int playerLevel,
        int requiredVocation, int minLevel)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: (byte)playerVocation,
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.MinimumLevel, minLevel)
            ]);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Vocation, new[] { (byte)requiredVocation });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeFalse();
    }

    [InlineData(2, 10, 2, 10)]
    [InlineData(2, 11, 2, 10)]
    [InlineData(5, 0, 5, 0)]
    [Theory]
    public void CanBeDressed_PlayerHasBothVocationAndLevel_ReturnsTrue(int playerVocation, int playerLevel,
        int requiredVocation, int minLevel)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: (byte)playerVocation,
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.MinimumLevel, minLevel)
            ]);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Vocation, new[] { (byte)requiredVocation });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    [InlineData(10, 10)]
    [InlineData(2, 1)]
    [InlineData(0, 0)]
    [Theory]
    public void CanBeDressed_ItemDoesNotRequireVocationButLevel_ReturnsTrue(int playerLevel, int minLevel)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: 1,
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.MinimumLevel, minLevel)
            ]);

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    [InlineData(10, 10)]
    [InlineData(2, 2)]
    [InlineData(0, 0)]
    [Theory]
    public void CanBeDressed_ItemRequiresVocationButNoLevel_ReturnsTrue(int playerVocation, int requiredVocation)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: (byte)playerVocation,
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, 1)
            });
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body")
            ]);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Vocation, new[] { (byte)requiredVocation });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    #endregion
}