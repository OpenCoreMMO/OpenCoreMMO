using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
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
using NeoServer.Domain.Tests.Helpers.Services;
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
        var sut = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Range, range),
                (ItemTypeAttribute.Attack, attack),
                (ItemTypeAttribute.Defense, defense),
                (ItemTypeAttribute.HitChance, chance)
            ]);

        //assert
        sut.InspectionText.Should().Be(expected);
    }

    [Theory]
    [InlineData(ItemTypeAttribute.ElementFire, 5, "(Atk: 6 + 5 fire, Def: 7)")]
    [InlineData(ItemTypeAttribute.ElementEarth, 10, "(Atk: 6 + 10 earth, Def: 7)")]
    [InlineData(ItemTypeAttribute.ElementEnergy, 1, "(Atk: 6 + 1 energy, Def: 7)")]
    [InlineData(ItemTypeAttribute.ElementIce, 23, "(Atk: 6 + 23 ice, Def: 7)")]
    [InlineData(ItemTypeAttribute.ElementIce, 0, "(Atk: 6, Def: 7)")]
    public void InspectionText_HasElementalDamage_ReturnsText(ItemTypeAttribute itemAttribute, int elementalDamage,
        string expected)
    {
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, itemTypeAttributes:
        [
            (ItemTypeAttribute.Attack, 6),
            (ItemTypeAttribute.Defense, 7),
            (itemAttribute, elementalDamage)
        ]);

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

        var map = MapTestDataBuilder.Build(tile, enemyTile);

        var spear = (ThrowableWeapon)ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 6),
                (ItemTypeAttribute.Defense, 7),
                (ItemTypeAttribute.HitChance, 100),
                (ItemTypeAttribute.Range, 3)
            ]);

        spear.Metadata.Attributes.SetCustomAttribute("breakChance", 100);

        player.Inventory.AddItem(spear, (byte)Slot.Left);

        tile.AddCreature(player);
        enemyTile.AddCreature(enemy);

        var attackService = AttackServiceTestBuilder.Build(map, combatConfig: new CombatConfiguration
        {
            InfiniteAmmo = false,
            InfiniteThrowingWeapon = false
        });

        //act

        attackService.Execute(new AttackInput(player, enemy, new CombatParameter
        {
            UsingWeapon = true,
            DamageType = DamageType.Melee,
            MaxDamage = 100,
            MinDamage = 100,
            HitChance = 100,
            Range = 3
        }));

        //assert
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
        var map = MapTestDataBuilder.Build(tile, enemyTile);

        var spear = (ThrowableWeapon)ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 6),
                (ItemTypeAttribute.Defense, 7),
                (ItemTypeAttribute.HitChance, 100),
                (ItemTypeAttribute.Range, 3)
            ]);

        player.Inventory.AddItem(spear, (byte)Slot.Left);

        tile.AddCreature(player);
        enemyTile.AddCreature(enemy);

        var attackService = AttackServiceTestBuilder.Build(map, combatConfig: new CombatConfiguration
        {
            InfiniteAmmo = false,
            InfiniteThrowingWeapon = false
        });

        //act

        var result = attackService.Execute(new AttackInput(player, enemy, new CombatParameter
        {
            UsingWeapon = true,
            DamageType = DamageType.Melee,
            MaxDamage = 100,
            MinDamage = 100,
            HitChance = 100,
            Range = 3
        }));

        //assert
        result.Result.Failed.Should().BeTrue();
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
        var sut = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
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
        var sut = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
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

    [Fact]
    public void CanBeDressed_ItemHasNoRequiredVocation_ReturnsTrue()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(vocationType: 1);
        var sut = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body")
            ]);

        //act
        var actual = sut.CanBeDressed(player);

        //assert
        actual.Should().BeTrue();
    }

    #endregion
}