using Moq;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Items.Items.Attributes;

public class ProtectionTest
{
    [Fact]
    public void Player_with_fire_protection_equipment_takes_reduced_fire_damage()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, itemTypeAttributes:
        [
            (ItemTypeAttribute.BodyPosition, "body"),
            (ItemTypeAttribute.AbsorbPercentFire, 20)
        ], charges: 10);

        var player = PlayerTestDataBuilder.Build(hp: 400);
        var enemy = PlayerTestDataBuilder.Build();

        player.Inventory.AddItem(sut, Slot.Body);

        sut.DressedIn(player);

        var damage = new CombatDamage(200, DamageType.Fire);
        player.TakeDamage(enemy, damage);

        damage.Damage.Should().Be(160);
    }


    [Fact]
    public void Player_with_no_fire_protection_equipment_takes_regular_fire_damage()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, itemTypeAttributes:
        [
            (ItemTypeAttribute.BodyPosition, "body"),
            (ItemTypeAttribute.AbsorbPercentPoison, 20)
        ], charges: 10);

        var player = PlayerTestDataBuilder.Build(hp: 400);
        var enemy = PlayerTestDataBuilder.Build();

        player.Inventory.AddItem(sut, Slot.Body);

        sut.DressedIn(player);

        var damage = new CombatDamage(200, DamageType.Fire);
        player.TakeDamage(enemy, damage);

        damage.Damage.Should().Be(200);
    }

    [Fact]
    public void Player_with_100percent_fire_protection_equipment_takes_no_fire_damage()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, itemTypeAttributes:
        [
            (ItemTypeAttribute.BodyPosition, "body"),
            (ItemTypeAttribute.AbsorbPercentFire, 100)
        ], charges: 10);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        player.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(player);

        var damage = new CombatDamage(200, DamageType.Fire);
        player.TakeDamage(enemy, damage);

        damage.Damage.Should().Be(0);
    }


    [Fact]
    public void Player_with_fire_protection_equipment_takes_regular_energy_damage()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "body", itemTypeAttributes:
        [
            (ItemTypeAttribute.AbsorbPercentFire, 100)
        ], charges: 10);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        player.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(player);

        var damage = new CombatDamage(100, DamageType.Energy);
        player.TakeDamage(enemy, damage);

        damage.Damage.Should().Be(100);
    }

    [Fact]
    public void Player_takes_regular_damage_after_removing_protection_equipment()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, itemTypeAttributes:
        [
            (ItemTypeAttribute.BodyPosition, "body"),
            (ItemTypeAttribute.AbsorbPercentFire, 100)
        ], charges: 10);

        var player = PlayerTestDataBuilder.Build(hp: 500);
        var enemy = PlayerTestDataBuilder.Build();

        player.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(player);

        var damage = new CombatDamage(200, DamageType.Fire);
        player.TakeDamage(enemy, damage);

        damage.Damage.Should().Be(0);

        player.Inventory.RemoveItem(Slot.Body, 1);
        sut.UndressFrom(player);

        var secondDamage = new CombatDamage(200, DamageType.Fire);
        player.TakeDamage(enemy, secondDamage);

        secondDamage.Damage.Should().Be(200);
    }

    [Fact]
    public void Player_equipment_loses_charge_when_absorbing_damage()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var enemy = PlayerTestDataBuilder.Build();
        var defender = PlayerTestDataBuilder.Build();

        (map[101, 100, 7] as DynamicTile)?.AddCreature(defender);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 50,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.AbsorbPercentEnergy, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(100, DamageType.Energy);
        defender.TakeDamage(enemy, damage);

        //assert
        sut.Charges.Should().Be(49);
    }

    [Fact]
    public void Player_equipment_does_not_loses_charge_when_not_absorbing_damage()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var defender = PlayerTestDataBuilder.Build();
        var attacker = PlayerTestDataBuilder.Build();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(attacker);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(defender);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 50,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "body"),
                (ItemTypeAttribute.AbsorbPercentFire, 100)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(100, DamageType.Energy);
        defender.TakeDamage(attacker, damage);

        //assert
        sut.Charges.Should().Be(50);
    }

    [Fact]
    public void Player_with_defense_equipment_with_infinite_charges_blocks_all_damage()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var defender = PlayerTestDataBuilder.Build();
        var attacker = PlayerTestDataBuilder.Build();
        var oldHp = defender.HealthPoints;

        (map[100, 100, 7] as DynamicTile)?.AddCreature(attacker);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(defender);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 0, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(100, DamageType.Energy);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(0);
        defender.HealthPoints.Should().Be(oldHp);
    }

    [Fact]
    public void Player_wearing_protection_item_without_any_charges_will_not_protect_against_damages()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var defender = PlayerTestDataBuilder.Build(hp: 5000);
        var attacker = PlayerTestDataBuilder.Build();
        var oldHp = defender.HealthPoints;

        (map[100, 100, 7] as DynamicTile)?.AddCreature(attacker);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(defender);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1, slot: "body");
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.AbsorbPercentEnergy, 100);

        sut.DecreaseCharges();

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var combatDamage = new CombatDamage(100, DamageType.Energy);
        defender.TakeDamage(attacker, combatDamage);

        //assert
        combatDamage.Damage.Should().NotBe(0);
        defender.HealthPoints.Should().BeLessThan(oldHp);
    }

    [Fact]
    public void Player_with_equipment_with_1_protection_charge_reduces_taken_damage()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var defender = PlayerTestDataBuilder.Build();
        var attacker = PlayerTestDataBuilder.Build();

        (map[100, 100, 7] as DynamicTile)?.AddCreature(attacker);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(defender);

        var oldHp = defender.HealthPoints;

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1, slot: "body", itemTypeAttributes:
        [
            (ItemTypeAttribute.AbsorbPercentEnergy, 100),
            (ItemTypeAttribute.Duration, 100)
        ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(100, DamageType.Energy);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(0);
        defender.HealthPoints.Should().Be(oldHp);
    }

    [Theory]
    [InlineData(-100, 400, 100)]
    [InlineData(-50, 300, 200)]
    [InlineData(-5, 210, 290)]
    public void Player_with_negative_damage_protection_equipment_increases_damage(sbyte protection,
        ushort expectedDamage, ushort remainingHp)
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentEnergy, protection)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, DamageType.Energy);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(expectedDamage);
        defender.HealthPoints.Should().Be(remainingHp);
    }

    [Fact]
    public void Player_with_mana_drain_protection_equipment_decreases_damage()
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentManaDrain, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, DamageType.ManaDrain);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(180);
        defender.HealthPoints.Should().Be(500);
        defender.Mana.Should().Be(320);
    }

    [Fact]
    public void Player_with_life_drain_protection_equipment_decreases_damage()
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();
        var oldHp = defender.HealthPoints;

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentLifeDrain, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, DamageType.LifeDrain);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(180);
        defender.HealthPoints.Should().Be(320);
    }

    [Theory]
    [InlineData(DamageType.Energy, ItemTypeAttribute.AbsorbPercentEnergy)]
    [InlineData(DamageType.Fire, ItemTypeAttribute.AbsorbPercentFire)]
    [InlineData(DamageType.Drown, ItemTypeAttribute.AbsorbPercentDrown)]
    [InlineData(DamageType.Holy, ItemTypeAttribute.AbsorbPercentHoly)]
    [InlineData(DamageType.Ice, ItemTypeAttribute.AbsorbPercentIce)]
    [InlineData(DamageType.ManaDrain, ItemTypeAttribute.AbsorbPercentManaDrain)]
    [InlineData(DamageType.Earth, ItemTypeAttribute.AbsorbPercentPoison)]
    [InlineData(DamageType.Death, ItemTypeAttribute.AbsorbPercentDeath)]
    [InlineData(DamageType.LifeDrain, ItemTypeAttribute.AbsorbPercentLifeDrain)]
    [InlineData(DamageType.Physical, ItemTypeAttribute.AbsorbPercentPhysical)]
    [InlineData(DamageType.Melee, ItemTypeAttribute.AbsorbPercentPhysical)]
    public void Player_with_elemental_damage_protection_equipment_decreases_damage(DamageType damageType,
        ItemTypeAttribute protectionAttribute)
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (protectionAttribute, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, damageType);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(180);
    }

    [Theory]
    [InlineData(DamageType.Energy, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Fire, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Drown, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Holy, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Ice, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.ManaDrain, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Earth, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Death, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.LifeDrain, ItemTypeAttribute.AbsorbPercentElements)]
    [InlineData(DamageType.Physical, ItemTypeAttribute.AbsorbPercentElements, 200)]
    [InlineData(DamageType.Melee, ItemTypeAttribute.AbsorbPercentElements, 200)]
    public void Player_with_all_elemental_damage_protection_equipment_decreases_damage(DamageType damageType,
        ItemTypeAttribute protectionAttribute, ushort expectedDamage = 180)
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (protectionAttribute, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, damageType);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(expectedDamage);
    }

    [Theory]
    [InlineData(DamageType.Energy, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Fire, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Drown, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Holy, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Ice, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.ManaDrain, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Earth, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Death, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.LifeDrain, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Physical, ItemTypeAttribute.AbsorbPercentAll)]
    [InlineData(DamageType.Melee, ItemTypeAttribute.AbsorbPercentAll)]
    public void Player_with_all_damage_protection_equipment_decreases_damage(DamageType damageType,
        ItemTypeAttribute protectionAttribute)
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (protectionAttribute, 10)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, damageType);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(180);
    }


    [Fact]
    public void Player_with_all_and_death_damage_protection_equipment_decreases_damage()
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentAll, 10),
                (ItemTypeAttribute.AbsorbPercentDeath, 50)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, DamageType.Death);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(100);

        //act
        var fireDamage = new CombatDamage(200, DamageType.Fire);
        defender.TakeDamage(attacker, fireDamage);

        //assert
        fireDamage.Damage.Should().Be(180);
    }

    [Fact]
    public void Player_with_elements_and_death_damage_protection_equipment_decreases_damage()
    {
        //arrange
        var defender = PlayerTestDataBuilder.Build(hp: 500, mana: 500);
        var attacker = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentElements, 10),
                (ItemTypeAttribute.AbsorbPercentDeath, 50)
            ]);

        defender.Inventory.AddItem(sut, Slot.Body);
        sut.DressedIn(defender);

        //act
        var damage = new CombatDamage(200, DamageType.Death);
        defender.TakeDamage(attacker, damage);

        //assert
        damage.Damage.Should().Be(100);

        //act
        var fireDamage = new CombatDamage(200, DamageType.Fire);
        defender.TakeDamage(attacker, fireDamage);

        //assert
        fireDamage.Damage.Should().Be(180);

        //act
        var meleeDamage = new CombatDamage(200, DamageType.Melee);
        defender.TakeDamage(attacker, meleeDamage);

        //assert
        meleeDamage.Damage.Should().Be(200);
    }

    [Fact]
    public void Equipment_look_text_shows_correct_protection_text()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10, slot: "body",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentEnergy, 10),
                (ItemTypeAttribute.AbsorbPercentFire, 20),
                (ItemTypeAttribute.AbsorbPercentDeath, -25),
                (ItemTypeAttribute.AbsorbPercentManaDrain, 30),
                (ItemTypeAttribute.AbsorbPercentLifeDrain, 45),
                (ItemTypeAttribute.AbsorbPercentIce, 50),
                (ItemTypeAttribute.AbsorbPercentPhysical, -65),
                (ItemTypeAttribute.AbsorbPercentDrown, 80),
                (ItemTypeAttribute.AbsorbPercentPoison, 100),
                (ItemTypeAttribute.AbsorbPercentHoly, 50)
            ]);

        var sut = new Protection(item);
        //assert
        sut.ToString().Should()
            .Be(
                "protection energy +10%, fire +20%, death -25%, mana drain +30%, life drain +45%, ice +50%, physical -65%, drown +80%, earth +100%, holy +50%");
    }

    [Fact]
    public void ToString_AllProtection_ReturnsLookText()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentAll, 10)
            ]);
        var sut = new Protection(item);

        //assert
        sut.ToString().Should().Be("protection all +10%");
    }

    [Fact]
    public void ToString_ElementalProtection_ReturnsLookText()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentElements, 10)
            ]);
        var sut = new Protection(item);

        //assert
        sut.ToString().Should().Be("protection elemental +10%");
    }

    [Fact]
    public void ToString_0Protection_Ignores()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentElements, 10),
                (ItemTypeAttribute.AbsorbPercentDeath, 0)
            ]);
        var sut = new Protection(item);

        //assert
        sut.ToString().Should().Be("protection elemental +10%");
    }


    [Fact]
    public void Protect_NoDamageProtection_DoNotProtect()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1);
        var combatDamage = new CombatDamage(100, DamageType.Energy);

        var sut = new Protection(item);

        //act
        sut.Protect(combatDamage);

        //assert
        combatDamage.Damage.Should().Be(100);
    }

    [Fact]
    public void Protect_DamageProtectionsNull_DoNotProtect()
    {
        //arrange
        var item = new Mock<IItem>();
        var itemType = new Mock<IItemType>();
        var itemAttribute = new ItemTypeAttributeList();

        itemType.SetupGet(x => x.Attributes).Returns(itemAttribute);
        item.Setup(x => x.Metadata).Returns(itemType.Object);

        var combatDamage = new CombatDamage(100, DamageType.Energy);

        var sut = new Protection(item.Object);

        //act
        sut.Protect(combatDamage);

        //assert
        combatDamage.Damage.Should().Be(100);
    }

    [Fact]
    public void Protect_DamageAsNone_DoNotProtect()
    {
        //arrange
        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.AbsorbPercentElements, 10),
                (ItemTypeAttribute.AbsorbPercentDeath, 0)
            ]);
        var combatDamage = new CombatDamage(100, DamageType.None);

        var sut = new Protection(item);

        //act
        sut.Protect(combatDamage);

        //assert
        combatDamage.Damage.Should().Be(100);
    }
}