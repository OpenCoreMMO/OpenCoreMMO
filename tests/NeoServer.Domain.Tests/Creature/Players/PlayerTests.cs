using AutoFixture;
using Moq;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerTests
{
    [Theory]
    [InlineData(100, 111, true)]
    [InlineData(100, 112, false)]
    [InlineData(105, 106, true)]
    [InlineData(95, 94, true)]
    [InlineData(94, 94, false)]
    public void CanMoveThing_Given_Distance_Bigger_Than_11_Returns_False(ushort toX, ushort toY, bool expected)
    {
        var sut = new Player(
            1,
            "PlayerA",
            ChaseMode.Stand,
            100,
            100,
            100,
            new Vocation(),
            new Group(),
            Gender.Male,
            true, 30, 30,
            FightMode.Attack,
            100,
            100,
            new Dictionary<SkillType, Skill>
            {
                { SkillType.Axe, new Skill(SkillType.Axe, 10) }
            },
            new Dictionary<uint, int>(),
            300,
            new Outfit(),
            300,
            new Location(100, 100, 7),
            null,
            null)
        {
            LastLogOut = DateTime.UtcNow
        };

        Assert.Equal(expected, sut.CanMoveThing(new Location(toX, toY, 7)));
    }

    [Fact]
    public void OnDamage_When_Receives_Melee_Attack_Reduce_Health()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.OnDamage(enemy, new CombatDamageList(new CombatDamage(5, DamageType.Melee)));

        Assert.Equal((uint)95, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_When_Receives_Mana_Attack_Reduce_Mana()
    {
        var sut = PlayerTestDataBuilder.Build(mana: 30) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.OnDamage(enemy, new CombatDamageList(new CombatDamage(5, DamageType.ManaDrain)));

        Assert.Equal((uint)25, sut.Mana);
    }

    [Fact]
    public void OnDamage_With_ManaShield_Enabled_When_Damage_Less_Than_Mana_Reduce_Only_Mana()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, mana: 100) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.EnableManaShield();

        sut.OnDamage(enemy, new CombatDamageList(new CombatDamage(50, DamageType.Melee)));

        Assert.Equal((uint)50, sut.Mana);
        Assert.Equal((uint)100, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_With_ManaShield_Enabled_When_Damage_Greater_Than_Mana_Reduce_Mana_And_Health()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, mana: 100) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.EnableManaShield();

        sut.OnDamage(enemy, new CombatDamageList(new CombatDamage(150, DamageType.Melee)));

        Assert.Equal((uint)0, sut.Mana);
        Assert.Equal((uint)50, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_With_ManaShield_Enabled_When_Damage_Equal_To_Mana_Reduce_Only_Mana()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, mana: 100) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.EnableManaShield();

        sut.OnDamage(enemy, new CombatDamageList(new CombatDamage(100, DamageType.Melee)));

        Assert.Equal((uint)0, sut.Mana);
        Assert.Equal((uint)100, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_With_ManaShield_Enabled_When_Receiving_Melee_And_ManaDrain_Reduce_Mana()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, mana: 100) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.EnableManaShield();

        sut.OnDamage(enemy,
            new CombatDamageList([new CombatDamage(50, DamageType.Melee), new CombatDamage(50, DamageType.ManaDrain)]));

        Assert.Equal((uint)0, sut.Mana);
        Assert.Equal((uint)100, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_With_ManaShield_Enabled_And_Zero_Mana_When_Receiving_Melee_And_ManaDrain_Reduce_Health()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, mana: 0) as Player;
        var enemy = PlayerTestDataBuilder.Build() as Player;
        sut.EnableManaShield();

        sut.OnDamage(enemy,
            new CombatDamageList([new CombatDamage(50, DamageType.Melee), new CombatDamage(50, DamageType.ManaDrain)]));

        Assert.Equal((uint)0, sut.Mana);
        Assert.Equal((uint)50, sut.HealthPoints);
    }

    [Fact]
    public void FlagIsEnabled_Enabled_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.Group.EnableFlag(PlayerFlag.IgnoreYellCheck);
        var result = sut.Group.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);

        result.Should().BeTrue();
    }

    [Fact]
    public void FlagIsEnabled_Disabled_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        var result = sut.Group.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);

        result.Should().BeFalse();
    }

    [Fact]
    public void SetFightMode_ChangesMode()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.ChangeFightMode(FightMode.Defense);

        sut.FightMode.Should().Be(FightMode.Defense);
        sut.ChangeFightMode(FightMode.Attack);

        sut.FightMode.Should().Be(FightMode.Attack);
    }

    [Fact]
    public void ChangeChaseMode_ChangesChaseMode()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.ChangeChaseMode(ChaseMode.Stand);

        sut.ChaseMode.Should().Be(ChaseMode.Stand);
        sut.ChangeChaseMode(ChaseMode.Follow);

        sut.ChaseMode.Should().Be(ChaseMode.Follow);
    }

    [ThreadBlocking]
    [Fact]
    public void ChangeChaseMode_Follow_InvokeFollow()
    {
        var sut = PlayerTestDataBuilder.Build(pathFinder: new Mock<IPathFinder>().Object);
        var enemy = PlayerTestDataBuilder.Build();

        var called = false;
        EventAggregatorTestHelper.SetupEventAggregator<CreatureStartedFollowingEvent>(_ => called = true);

        sut.ChangeChaseMode(ChaseMode.Stand);

        sut.SetAttackTarget(enemy);

        called.Should().BeFalse();

        sut.ChangeChaseMode(ChaseMode.Follow);

        called.Should().BeTrue();
    }

    [Fact]
    public void ChangeChaseMode_Stand_SetsFollowingToFalse()
    {
        var enemy = PlayerTestDataBuilder.Build();
        var pathFinder = new Mock<IPathFinder>().Object;

        var sut = PlayerTestDataBuilder.Build(pathFinder: pathFinder);

        sut.SetAttackTarget(enemy);

        sut.ChangeChaseMode(ChaseMode.Follow);

        sut.IsFollowing.Should().BeTrue();

        sut.ChangeChaseMode(ChaseMode.Stand);

        sut.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void ChangeSecureMode_ChangesSecureMode()
    {
        var sut = PlayerTestDataBuilder.Build();

        sut.ChangeSecureMode(PvpSecureMode.PvPEnabled);
        sut.SecureMode.Should().Be(PvpSecureMode.PvPEnabled);
        sut.ChangeSecureMode(PvpSecureMode.PvPDisabled);
        sut.SecureMode.Should().Be(PvpSecureMode.PvPDisabled);
    }

    [Fact]
    public void KnowsCreatureWithId_DontKnow_ReturnsFalse()
    {
        var fixture = new Fixture();
        var sut = PlayerTestDataBuilder.Build();

        var unknownCreature = fixture.Create<uint>();

        var actual = sut.KnowsCreatureWithId(unknownCreature);
        actual.Should().BeFalse();
    }

    [Fact]
    public void KnowsCreatureWithId_Knows_ReturnsTrue()
    {
        var fixture = new Fixture();
        var sut = PlayerTestDataBuilder.Build();

        var knownCreature = fixture.Create<uint>();

        sut.AddKnownCreature(knownCreature);

        var actual = sut.KnowsCreatureWithId(knownCreature);
        actual.Should().BeTrue();
    }

    [Fact]
    public void Player_direction_cannot_be_set_to_none()
    {
        //arrange
        var sut = PlayerTestDataBuilder.Build();
        sut.TurnTo(Direction.North);

        var fromTile = MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var toTile = MapTestDataBuilder.CreateTile(new Location(100, 100, 8));

        //act
        sut.OnMoved(fromTile, toTile, []);

        //assert
        sut.Direction.Should().Be(Direction.North);
    }

    [Fact]
    public void SetAsHungry_replaces_regeneration_with_hungry_condition()
    {
        var sut = PlayerTestDataBuilder.Build();
        var regenerationCondition = new Condition(ConditionType.Regeneration, 10000);
        sut.AddCondition(regenerationCondition);

        sut.SetAsHungry();

        sut.HasCondition(ConditionType.Regeneration).Should().BeFalse();
        sut.HasCondition(ConditionType.Hungry).Should().BeTrue();
    }

    [Fact]
    public void SetAsHungry_does_not_invoke_endaction_of_removed_regeneration_condition()
    {
        var endActionInvoked = false;
        var sut = PlayerTestDataBuilder.Build();
        var regenerationCondition = new Condition(ConditionType.Regeneration, 10000, () => endActionInvoked = true);
        sut.AddCondition(regenerationCondition);

        sut.SetAsHungry();

        endActionInvoked.Should().BeFalse();
    }

    [Fact]
    public void SetAsHungry_does_not_stack_overflow_when_regeneration_has_endaction_set_to_setashungry()
    {
        var sut = PlayerTestDataBuilder.Build();
        var regenerationCondition = new Condition(ConditionType.Regeneration, 10000, sut.SetAsHungry);
        sut.AddCondition(regenerationCondition);

        sut.SetAsHungry();
        sut.HasCondition(ConditionType.Regeneration).Should().BeFalse();
        sut.HasCondition(ConditionType.Hungry).Should().BeTrue();
    }
}