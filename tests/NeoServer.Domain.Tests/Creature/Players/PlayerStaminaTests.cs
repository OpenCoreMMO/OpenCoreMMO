using Moq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerStaminaTests
{
    [Fact]
    public void Player_consumes_stamina_when_gain_exp()
    {
        //assert
        var player = PlayerTestDataBuilder.Build();
        //act
        player.GainExperience(1);
        //result
        Assert.Equal(42 * 60 - 1, player.StaminaMinutes);
    }

    [Fact]
    public void Stamina_gets_0_when_consuming_more_than_available()
    {
        //assert
        var player = PlayerTestDataBuilder.Build(stamina: 0);
        //act
        player.GainExperience(1);
        //result
        Assert.Equal(0, player.StaminaMinutes);
    }

    [Fact]
    public void Player_gains_half_of_experience_when_stamina_is_low()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES);
        var initialExp = player.Experience;
        // act
        player.GainExperience(10);
        // assert
        Assert.Equal(initialExp + 5, player.Experience);
    }


    [Fact]
    public void Loot_is_empty_when_aggressor_has_not_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var player =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES); // Enough stamina
        var monster = MonsterTestDataBuilder.Build();
        monster.ReceivedDamages.AddOrUpdateDamage(player, 100, unjustified: false);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().BeEmpty();
    }

    [Fact]
    public void Loot_is_not_empty_when_aggressor_has_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var player =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES + 1); // Enough stamina
        var monster = MonsterTestDataBuilder.Build();
        monster.ReceivedDamages.AddOrUpdateDamage(player, 100, unjustified: false);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void Loot_is_not_empty_when_there_are_at_least_one_aggressor_with_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var player1 =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES + 1); // Enough stamina

        var player2 =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES); // Enough stamina

        var monster = MonsterTestDataBuilder.Build(maxHealth: 200);
        monster.ReceivedDamages.AddOrUpdateDamage(player1, 100, unjustified: false);
        monster.ReceivedDamages.AddOrUpdateDamage(player2, 100, unjustified: false);

        var party = new Party.Party(player1, new ChatChannel(1, "party channel"));
        party.Invite(player1, player2);
        party.JoinPlayer(player2);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void Loot_is_empty_when_both_aggressors_have_not_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var player1 =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES); // Enough stamina

        var player2 =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES); // Enough stamina

        var monster = MonsterTestDataBuilder.Build(maxHealth: 200);
        monster.ReceivedDamages.AddOrUpdateDamage(player1, 100, unjustified: false);
        monster.ReceivedDamages.AddOrUpdateDamage(player2, 100, unjustified: false);

        var party = new Party.Party(player1, new ChatChannel(1, "party channel"));
        party.Invite(player1, player2);
        party.JoinPlayer(player2);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().BeEmpty();
    }

    [Fact]
    public void Loot_is_not_empty_when_aggressor_is_a_monster()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var monsterEnemy =
            MonsterTestDataBuilder.Build();

        var monster = MonsterTestDataBuilder.Build(maxHealth: 200);
        monster.ReceivedDamages.AddOrUpdateDamage(monsterEnemy, 200, unjustified: false);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void Loot_is_empty_when_aggressor_is_a_summon_of_a_player_without_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var aggressor =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES); // not Enough stamina
        var summon =
            MonsterTestDataBuilder.BuildSummon(aggressor);

        var monster = MonsterTestDataBuilder.Build(maxHealth: 200);
        monster.ReceivedDamages.AddOrUpdateDamage(summon, 200, unjustified: false);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().BeEmpty();
    }

    [Fact]
    public void Loot_is_not_empty_when_aggressor_is_a_summon_of_a_player_with_enough_stamina()
    {
        // Arrange
        var gameConfig = new GameConfiguration { LootRate = 1 };
        var mockItemFactory = new Mock<IItemFactory>();

        var expectedItem = ItemTestDataBuilder.CreateWeaponItem(2);

        var lootService = new LootService(gameConfig, mockItemFactory.Object);

        var aggressor =
            PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES + 1); //Enough stamina
        var summon =
            MonsterTestDataBuilder.BuildSummon(aggressor);

        var monster = MonsterTestDataBuilder.Build(maxHealth: 200);
        monster.ReceivedDamages.AddOrUpdateDamage(summon, 200, unjustified: false);

        monster.Metadata.Loot = new Loot([new LootItem(expectedItem.Metadata, 1, uint.MaxValue, [])]);

        // Act
        var loot = lootService.GenerateLoot(monster);

        // Assert
        loot.Items.Should().NotBeEmpty();
    }

    [Fact]
    public void Player_with_premium_time_gain_50_percent_exp_bonus_when_in_stamina_bonus()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_BONUS_MINUTES, premiumTime: 1,
            experience: 0); //Enough stamina

        // Act
        aggressor.GainExperience(100);

        // Assert
        aggressor.Experience.Should().Be(150);
    }

    [Fact]
    public void Player_without_premium_time_gain_regular_exp_in_stamina_bonus()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_BONUS_MINUTES, premiumTime: 0,
            experience: 0); //Enough stamina

        // Act
        aggressor.GainExperience(100);

        // Assert
        aggressor.Experience.Should().Be(100);
    }

    [Fact]
    public void Player_with_premium_time_gain_regular_exp_when_below_stamina_bonus()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_THRESHOLD_MINUTES + 1,
            premiumTime: 1, experience: 0);

        // Act
        aggressor.GainExperience(100);

        // Assert
        aggressor.Experience.Should().Be(100);
    }

    [Fact]
    public void Player_gains_no_exp_when_at_0_stamina()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: 0, premiumTime: 1, experience: 0);

        // Act
        aggressor.GainExperience(100);

        // Assert
        aggressor.Experience.Should().Be(0);
    }

    [Fact]
    public void Player_restores_10_minutes_of_stamina_when_30_minutes_logged_out()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: 0, premiumTime: 1, experience: 0);
        aggressor.LastLogOut = DateTime.UtcNow.AddMinutes(-30);

        // Act
        aggressor.Login();

        // Assert
        aggressor.StaminaMinutes.Should().Be(30 / GameConstants.STAMINA_REGENERATION_EACH_MINUTES);
    }
    
    [Fact]
    public void Player_restores_10_minutes_of_stamina_when_60_minutes_logged_out_in_bonus_stamina()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_BONUS_MINUTES, premiumTime: 1, experience: 0);
        aggressor.LastLogOut = DateTime.UtcNow.AddMinutes(-60);

        // Act
        aggressor.Login();

        // Assert
        aggressor.StaminaMinutes.Should().Be(GameConstants.STAMINA_BONUS_MINUTES + 10);
    }
    
    [Fact]
    public void Player_does_not_restore_stamina_when_10_minutes_logged_out_or_less()
    {
        // Arrange
        var aggressor = PlayerTestDataBuilder.Build(stamina: GameConstants.STAMINA_BONUS_MINUTES, premiumTime: 1, experience: 0);
        aggressor.LastLogOut = DateTime.UtcNow.AddMinutes(-10);

        // Act
        aggressor.Login();

        // Assert
        aggressor.StaminaMinutes.Should().Be(GameConstants.STAMINA_BONUS_MINUTES);
    }
}