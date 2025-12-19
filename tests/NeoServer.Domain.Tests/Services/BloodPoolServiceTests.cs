using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Services;

/// <summary>
/// Unit tests for BloodPoolService.
/// Tests cover: splash creation on tile, damage-based splash creation, pool creation,
/// blood type color mapping, and edge cases.
/// </summary>
public class BloodPoolServiceTests
{
    #region Helper Methods

    private static LiquidPoolFactory CreateLiquidPoolFactory()
    {
        var splashItemType = new ItemType();
        splashItemType.SetId(2019);
        splashItemType.SetGroup((byte)ItemGroup.Splash);

        var poolItemType = new ItemType();
        poolItemType.SetId(2016);
        poolItemType.SetGroup((byte)ItemGroup.Splash);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(splashItemType, poolItemType);
        return new LiquidPoolFactory(itemTypeStore);
    }

    #endregion

    #region CreateSplash(ICreature) Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_splash_on_creature_tile()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        // Act
        service.CreateSplash(player);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeTrue("a blood splash should be added to the tile");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_red_splash_for_blood_type_creature()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build(); // Players have BloodType.Blood by default
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        // Act
        service.CreateSplash(player);

        // Assert
        var liquid = tile.AllItems.OfType<ILiquid>().FirstOrDefault();
        liquid.Should().NotBeNull();
        liquid!.LiquidColor.Should().Be(LiquidColor.Red, "blood type creatures should create red splashes");
    }

    #endregion

    #region CreateSplash(ICreature, CombatDamage) Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_splash_when_creature_takes_physical_damage()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build(hp: 200);
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        var damage = new CombatDamage(50, DamageType.Melee);

        // Act
        service.CreateSplash(player, damage);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeTrue("physical damage should create a blood splash");
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void BloodPoolService_does_not_create_splash_for_elemental_damage()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build(hp: 200);
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        var damage = new CombatDamage(50, DamageType.Fire);

        // Act
        service.CreateSplash(player, damage);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeFalse("elemental damage should not create blood splashes");
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void BloodPoolService_does_not_create_splash_when_damage_is_zero()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build(hp: 200);
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        var damage = new CombatDamage(0, DamageType.Melee);

        // Act
        service.CreateSplash(player, damage);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeFalse("zero damage should not create blood splashes");
    }

    #endregion

    #region CreatePool(ICreature) Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_pool_on_creature_tile()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        // Act
        service.CreatePool(player);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeTrue("a blood pool should be added to the tile");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_red_pool_for_blood_type_creature()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(player);

        // Act
        service.CreatePool(player);

        // Assert
        var liquid = tile.AllItems.OfType<ILiquid>().FirstOrDefault();
        liquid.Should().NotBeNull();
        liquid!.LiquidColor.Should().Be(LiquidColor.Red, "blood type creatures should create red pools");
    }

    #endregion

    #region Monster Blood Type Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void BloodPoolService_creates_splash_on_monster_tile()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var liquidPoolFactory = CreateLiquidPoolFactory();
        var service = new BloodPoolService(liquidPoolFactory);

        var monster = MonsterTestDataBuilder.Build(map: map);
        monster.SetNewLocation(new Location(105, 105, 7));

        var tile = map[105, 105, 7] as DynamicTile;
        tile!.AddCreature(monster);

        // Act
        service.CreateSplash(monster);

        // Assert
        var hasLiquid = tile.AllItems.Any(item => item is ILiquid);
        hasLiquid.Should().BeTrue("monsters should also create blood splashes");
    }

    #endregion
}

