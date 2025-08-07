using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Items;

public class DistanceWeaponTests
{
    [Fact]
    public void InspectionText_NoAttributeFound_ReturnsText()
    {
        var sut = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes: Array.Empty<(ItemTypeAttribute, IConvertible)>());

        //assert
        sut.InspectionText.Should().BeEmpty();
    }


    [Theory]
    [InlineData(6, 7, 3, "(Range: 6, Atk: +7, Hit% +3)")]
    [InlineData(6, 0, 3, "(Range: 6, Hit% +3)")]
    [InlineData(6, 7, 0, "(Range: 6, Atk: +7)")]
    [InlineData(6, 0, 0, "(Range: 6)")]
    [InlineData(0, 7, 3, "(Atk: +7, Hit% +3)")]
    [InlineData(0, 7, -3, "(Atk: +7, Hit% -3)")]
    [InlineData(0, 7, 0, "(Atk: +7)")]
    [InlineData(0, 0, 3, "(Hit% +3)")]
    [InlineData(0, 0, 0, "")]
    public void InspectionText_AttributeFound_ReturnsText(int range, int attack, int chance, string expected)
    {
        var sut = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Range, range),
                (ItemTypeAttribute.Attack, attack),
                (ItemTypeAttribute.HitChance, chance)
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
        var sut = (IDistanceWeapon)ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.BodyPosition, "body")
            });
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
            skills: new Dictionary<SkillType, ISkill>
            {
                [SkillType.Level] = new Skill(SkillType.Level, (ushort)playerLevel)
            });
        var sut = (IDistanceWeapon)ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.MinimumLevel, minLevel)
            });
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
        var sut = (IDistanceWeapon)ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.BodyPosition, "body")
            });

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    #endregion
}