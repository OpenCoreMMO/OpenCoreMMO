using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items;

public class AmmoTests
{
    [Theory]
    [InlineData(6, "(Atk: 6)")]
    [InlineData(10, "(Atk: 10)")]
    [InlineData(1, "(Atk: 1)")]
    public void InspectionText_ReturnsText(int attack, string expected)
    {
        var sut = ItemTestDataBuilder.CreateAmmo(1, 10, [
            (ItemTypeAttribute.Attack, attack)
        ]);

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    [Theory]
    [InlineData(ItemTypeAttribute.ElementFire, 5, "(Atk: 6 + 5 fire)")]
    [InlineData(ItemTypeAttribute.ElementEarth, 10, "(Atk: 6 + 10 earth)")]
    [InlineData(ItemTypeAttribute.ElementEnergy, 1, "(Atk: 6 + 1 energy)")]
    [InlineData(ItemTypeAttribute.ElementIce, 23, "(Atk: 6 + 23 ice)")]
    [InlineData(ItemTypeAttribute.ElementIce, 0, "(Atk: 6)")]
    public void InspectionText_HasElementalDamage_ReturnsText(ItemTypeAttribute itemAttribute, int elementalDamage,
        string expected)
    {
        var sut = (IEquipment)ItemTestDataBuilder.CreateAmmo(1, 10, [
            (ItemTypeAttribute.Attack, 6),
            (itemAttribute, elementalDamage)
        ]);

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    #region CanBeDressed Tests

    [InlineData(2, 1)]
    [InlineData(2, 3)]
    [Theory]
    public void CanBeDressed_PlayerHasNotRequiredVocation_ReturnsFalse(int playerVocation,
        int requiredVocation)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: (byte)playerVocation);
        var sut = (IEquipment)ItemTestDataBuilder.CreateAmmo(1, 100, [
            (ItemTypeAttribute.BodyPosition, "body")
        ]);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Vocation, new[] { (byte)requiredVocation });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeFalse();
    }

    [InlineData(2, 1, 2, 10)]
    [InlineData(2, 8, 2, 10)]
    [InlineData(5, 0, 5, 0)]
    [InlineData(5, 1, 5, 1)]
    [Theory]
    public void CanBeDressed_PlayerHasVocationAndNoMinimumLevel_ReturnsTrue(int playerVocation, int playerLevel,
        int requiredVocation, int minLevel)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: (byte)playerVocation,
            skills: new Dictionary<SkillType, Skill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = (IEquipment)ItemTestDataBuilder.CreateAmmo(1, 100, [
            (ItemTypeAttribute.BodyPosition, "body"),
            (ItemTypeAttribute.MinimumLevel, minLevel)
        ]);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Vocation, new[] { (byte)requiredVocation });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void CanBeDressed_ItemHasNoRequiredVocation_ReturnsTrue()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: 1);
        var sut = (IEquipment)ItemTestDataBuilder.CreateAmmo(1, 100, [
            (ItemTypeAttribute.BodyPosition, "body")
        ]);

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    #endregion
}