using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items.Weapons;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Items.Items;

public class ThrowableWeaponTests
{
    [Theory]
    [InlineData(6, 7, 10, 3, "(Range: 6, Atk: 7, Def: 10, Hit% +3)")]
    [InlineData(0, 10, 1, 0, "(Atk: 10, Def: 1)")]
    [InlineData(0, 0, 0, 0, "(Atk: 0, Def: 0)")]
    public void InspectionText_AttributeFound_ReturnsText(int range, int attack, int defense, int chance,
        string expected)
    {
        var sut = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Range, range),
            (ItemAttribute.Attack, attack),
            (ItemAttribute.Defense, defense),
            (ItemAttribute.HitChance, chance)
        });

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    [Theory]
    [InlineData(ItemAttribute.ElementFire, 5, "(Atk: 6 + 5 fire, Def: 7)")]
    [InlineData(ItemAttribute.ElementEarth, 10, "(Atk: 6 + 10 earth, Def: 7)")]
    [InlineData(ItemAttribute.ElementEnergy, 1, "(Atk: 6 + 1 energy, Def: 7)")]
    [InlineData(ItemAttribute.ElementIce, 23, "(Atk: 6 + 23 ice, Def: 7)")]
    [InlineData(ItemAttribute.ElementIce, 0, "(Atk: 6, Def: 7)")]
    public void InspectionText_HasElementalDamage_ReturnsText(ItemAttribute itemAttribute, int elementalDamage,
        string expected)
    {
        var sut = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 6),
            (ItemAttribute.Defense, 7),
            (itemAttribute, elementalDamage)
        });

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    [Fact]
    public void Item_breaks_after_used()
    {
        //arrange

        var player = PlayerTestDataBuilder.Build();
        var enemy = MonsterTestDataBuilder.Build();

        var tile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var enemyTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 7));

        var spear = (ThrowableWeapon)ItemTestData.CreateThrowableDistanceItem(1,
            attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.Attack, 6),
                (ItemAttribute.Defense, 7),
                (ItemAttribute.HitChance, 100),
                (ItemAttribute.Range, 3)
            });

        spear.Metadata.Attributes.SetCustomAttribute("breakChance", 100);

        player.Inventory.AddItem(spear, (byte)Slot.Left);

        tile.AddCreature(player);
        enemyTile.AddCreature(enemy);

        //act
        var result = spear.Attack(player, enemy, out var combatResult);

        //assert
        result.Should().BeTrue();
        spear.Amount.Should().Be(0);
        player.Inventory[Slot.Left].Should().BeNull();
    }

    [Fact]
    public void Player_cannot_throw_spear_when_farther_than_3_tiles()
    {
        //arrange

        var player = PlayerTestDataBuilder.Build();
        var enemy = MonsterTestDataBuilder.Build();

        var tile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var enemyTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(104, 100, 7));

        var spear = (ThrowableWeapon)ItemTestData.CreateThrowableDistanceItem(1,
            attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.Attack, 6),
                (ItemAttribute.Defense, 7),
                (ItemAttribute.HitChance, 100),
                (ItemAttribute.Range, 3)
            });

        player.Inventory.AddItem(spear, (byte)Slot.Left);

        tile.AddCreature(player);
        enemyTile.AddCreature(enemy);

        //act
        var result = spear.Attack(player, enemy, out var combatResult);

        //assert
        result.Should().BeFalse();
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
        var sut = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
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
        var sut = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
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
        var sut = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
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