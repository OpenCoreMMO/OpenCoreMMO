using System.Collections.Immutable;
using System.Reflection;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items.Cumulatives;
using NeoServer.Domain.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Spawns;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;
using CreateItem = NeoServer.Domain.Common.Contracts.Items.CreateItem;

namespace NeoServer.Domain.Tests.MagicField;

public class MagicFieldTests
{
    [Fact]
    public void Ground_with_magic_field_injures_creature()
    {
        // Arrange: build fresh map/service per test
        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);
        var itemFactory = new TestItemFactory();
        var service = new MagicFieldService(map, itemFactory, new PvPConfiguration { PvpType = PvpType.OpenPvP });

        var location = new Location(100, 100, 7);
        var tile = (IDynamicTile)map[location];
        var tile2 = (IDynamicTile)map[new Location(100, 101, 7)];

        // Actor and low-HP victims so they die on field damage
        var actor = MonsterTestDataBuilder.Build(100, map: map);
        var dying1 = MonsterTestDataBuilder.Build(5, map: map);

        ((DynamicTile)tile).AddCreature(actor);
        ((DynamicTile)tile2).AddCreature(dying1);

        // Act
        var act1 = () => service.AddToGround(actor, tile, MagicFieldType.Fire);
        var act2 = () => service.AddToGround(actor, tile2, MagicFieldType.Fire);


        // Assert
        act1.Should().NotThrow();
        act2.Should().NotThrow();
        dying1.IsDead.Should().BeTrue();
    }

    [Fact]
    public void Ground_with_magic_field_injures_multiple_creatures()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var itemFactory = new TestItemFactory();
        var service = new MagicFieldService(map, itemFactory, new PvPConfiguration { PvpType = PvpType.OpenPvP });

        var location = new Location(105, 105, 7);
        var tile = (IDynamicTile)map[location];
        var actorTile = (IDynamicTile)map[new Location(105, 106, 7)];

        var actor = MonsterTestDataBuilder.Build(100, map: map);
        var healthy = MonsterTestDataBuilder.Build(100, map: map);
        var dying1 = MonsterTestDataBuilder.Build(5, map: map);
        var dying2 = MonsterTestDataBuilder.Build(5, map: map);

        ((DynamicTile)actorTile).AddCreature(actor);
        ((DynamicTile)tile).AddCreature(dying1);
        ((DynamicTile)tile).AddCreature(healthy);
        ((DynamicTile)tile).AddCreature(dying2);

        // Act
        var result = service.AddToGround(actor, tile, MagicFieldType.Fire);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        healthy.IsDead.Should().BeFalse();
        dying1.IsDead.Should().BeTrue();
        dying2.IsDead.Should().BeFalse();
    }

    [Fact]
    public void Ground_with_magic_field_fails_in_protection_zone()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var itemFactory = new TestItemFactory();
        var service = new MagicFieldService(map, itemFactory, new PvPConfiguration { PvpType = PvpType.OpenPvP });

        var location = new Location(105, 105, 7);
        var tile = (IDynamicTile)map[location];

        // Mark tile as protection zone and ensure actor is on a tile
        var flagsField = typeof(BaseTile).GetField("Flags", BindingFlags.NonPublic | BindingFlags.Instance);
        flagsField.SetValue(tile, (uint)TileFlags.ProtectionZone);
        var actor = MonsterTestDataBuilder.Build(100, map: map);
        ((DynamicTile)tile).AddCreature(actor);

        // Act
        var result = service.AddToGround(actor, tile, MagicFieldType.Fire);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(InvalidOperation.NotPermittedInProtectionZone);
    }

    [Fact]
    public void Ground_with_magic_field_reduces_damage_on_resistant_creature()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var itemFactory = new TestItemFactory();
        var service = new MagicFieldService(map, itemFactory, new PvPConfiguration { PvpType = PvpType.OpenPvP });

        var location = new Location(105, 105, 7);
        var tile = (IDynamicTile)map[location];
        var actorTile = (IDynamicTile)map[new Location(105, 106, 7)];

        var actor = MonsterTestDataBuilder.Build(100, map: map);
        ((DynamicTile)actorTile).AddCreature(actor);

        // Create monster with fire resistance
        var mapTool = new MapTool(map, new PathFinder(map));
        var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

        var resistantMonsterType = new MonsterType
        {
            Name = "Resistant Monster",
            MaxHealth = 50,
            Speed = 200,
            ElementResistance =
                ImmutableDictionary.Create<DamageType, sbyte>().Add(DamageType.Fire, 50) // 50% fire resistance
        };

        var resistantMonster = new Monster(resistantMonsterType, mapTool, spawnPoint);
        ((DynamicTile)tile).AddCreature(resistantMonster);

        var initialHealth = resistantMonster.HealthPoints;

        // Act
        var result = service.AddToGround(actor, tile, MagicFieldType.Fire);

        // Assert
        result.Succeeded.Should().BeTrue();
        resistantMonster.HealthPoints.Should().BeLessThan(initialHealth); // Some damage taken
        resistantMonster.HealthPoints.Should().BeGreaterThan(initialHealth - 20); // But reduced due to resistance
        resistantMonster.IsDead.Should().BeFalse(); // Should not die
    }

    [Fact]
    public void Ground_with_magic_field_increases_damage_on_weak_creature()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var itemFactory = new TestItemFactory();
        var service = new MagicFieldService(map, itemFactory, new PvPConfiguration { PvpType = PvpType.OpenPvP });

        var location = new Location(105, 105, 7);
        var tile = (IDynamicTile)map[location];
        var actorTile = (IDynamicTile)map[new Location(105, 106, 7)];

        var actor = MonsterTestDataBuilder.Build(100, map: map);
        ((DynamicTile)actorTile).AddCreature(actor);

        // Create monster with fire weakness
        var mapTool = new MapTool(map, new PathFinder(map));
        var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

        var weakMonsterType = new MonsterType
        {
            Name = "Weak Monster",
            MaxHealth = 30,
            Speed = 200,
            ElementResistance =
                ImmutableDictionary.Create<DamageType, sbyte>().Add(DamageType.Fire, -50) // 50% weakness
        };

        var weakMonster = new Monster(weakMonsterType, mapTool, spawnPoint);
        ((DynamicTile)tile).AddCreature(weakMonster);

        var initialHealth = weakMonster.HealthPoints;

        // Act
        var result = service.AddToGround(actor, tile, MagicFieldType.Fire);

        // Assert
        result.Succeeded.Should().BeTrue();
        weakMonster.HealthPoints.Should().BeLessThan(initialHealth); // Damage taken
        weakMonster.HealthPoints.Should().BeLessThan(initialHealth - 20); // Increased damage due to weakness
        weakMonster.IsDead.Should().BeTrue(); // Should die from increased damage
    }

    private sealed class TestItemFactory : IItemFactory
    {
        public event CreateItem OnItemCreated;

        public IItem Create(ushort typeId, Location location, int count = 1, IEnumerable<IItem> children = null)
        {
            return Create(typeId, location);
        }

        public IItem Create(ushort typeId, Location location,
            IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
            IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
            IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
            IDictionary<string, IConvertible> itemCustomAttributes = null, IEnumerable<IItem> children = null)
        {
            return Create(typeId, location);
        }

        public IItem Create(string name, Location location,
            IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
            IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
            IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
            IDictionary<string, IConvertible> itemCustomAttributes = null, IEnumerable<IItem> children = null)
        {
            return null;
        }

        public IEnumerable<Coin> CreateCoins(ulong amount)
        {
            return [];
        }

        public IItem CreateLootCorpse(ushort typeId, Location location, Loot loot, IThing killer)
        {
            return null;
        }

        public IItem Create(IItemType itemType, Location location,
            IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
            IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
            IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
            IDictionary<string, IConvertible> itemCustomAttributes = null, IEnumerable<IItem> children = null)
        {
            return null;
        }

        public IItem Create(ushort typeId, Location location)
        {
            var itemType = new ItemType();
            itemType.SetId(typeId);
            itemType.SetGroup((byte)ItemGroup.MagicField);

            // Configure damage high enough to kill low-HP monsters
            var attributes = itemType.Attributes;
            var field = new ItemTypeAttributeList();
            field.SetAttribute(ItemTypeAttribute.Type, DamageType.Fire);
            field.SetAttribute(ItemTypeAttribute.Damage, new object[] { 10, 20 });
            attributes.SetAttribute(ItemTypeAttribute.Field, "fire", field);

            return new Domain.Items.Items.MagicField(itemType, location);
        }
    }
}