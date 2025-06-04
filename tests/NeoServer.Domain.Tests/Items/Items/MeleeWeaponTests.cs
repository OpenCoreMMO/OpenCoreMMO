using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items;

public class MeleeWeaponTests
{
    [Theory]
    [InlineData(6, 7, 10, "(Atk: 6, Def: 7 +10)")]
    [InlineData(10, 1, 0, "(Atk: 10, Def: 1)")]
    [InlineData(25, 0, 5, "(Atk: 25, Def: 0 +5)")]
    [InlineData(0, 10, 5, "(Atk: 0, Def: 10 +5)")]
    [InlineData(0, 0, 5, "(Atk: 0, Def: 0 +5)")]
    [InlineData(0, 0, 0, "(Atk: 0, Def: 0)")]
    public void InspectionText_ReturnsText(int attack, int defense, int extraDef, string expected)
    {
        var sut = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, attack),
            (ItemAttribute.Defense, defense),
            (ItemAttribute.ExtraDefense, extraDef)
        });

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    [Theory]
    [InlineData(ItemAttribute.ElementFire, 5, "(Atk: 6 + 5 fire, Def: 7 +10)")]
    [InlineData(ItemAttribute.ElementEarth, 10, "(Atk: 6 + 10 earth, Def: 7 +10)")]
    [InlineData(ItemAttribute.ElementEnergy, 1, "(Atk: 6 + 1 energy, Def: 7 +10)")]
    [InlineData(ItemAttribute.ElementIce, 23, "(Atk: 6 + 23 ice, Def: 7 +10)")]
    [InlineData(ItemAttribute.ElementIce, 0, "(Atk: 6, Def: 7 +10)")]
    public void InspectionText_HasElementalDamage_ReturnsText(ItemAttribute itemAttribute, int elementalDamage,
        string expected)
    {
        var sut = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 6),
            (ItemAttribute.Defense, 7),
            (ItemAttribute.ExtraDefense, 10),
            (itemAttribute, elementalDamage)
        });

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
        var sut = (IWeapon)ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "body")
        });
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, new[] { (byte)requiredVocation });

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
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = (IWeapon)ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "body"),
            (ItemAttribute.MinimumLevel, minLevel)
        });
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, new[] { (byte)requiredVocation });

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
        var sut = (IWeapon)ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "body")
        });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    #endregion
}