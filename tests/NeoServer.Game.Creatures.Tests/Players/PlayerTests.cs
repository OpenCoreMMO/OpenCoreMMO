using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Moq;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;
using NeoServer.Game.Creatures.Vocation;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players;

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
        var sut = new Player.Player(
            1,
            "PlayerA",
            ChaseMode.Stand,
            100,
            100,
            100,
            new Vocation.Vocation(),
            new Group.Group(),
            Gender.Male,
            true, 30, 30,
            FightMode.Attack,
            100,
            100,
            new Dictionary<SkillType, ISkill>
            {
                { SkillType.Axe, new Skill(SkillType.Axe, 10) }
            },
            new Dictionary<int, int>(),
            300,
            new Outfit(),
            300,
            new Location(100, 100, 7),
            null,
            null);

        Assert.Equal(expected, sut.CanMoveThing(new Location(toX, toY, 7)));
    }

    [Fact]
    public void OnDamage_When_Receives_Melee_Attack_Reduce_Health()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100) as Player.Player;
        var enemy = PlayerTestDataBuilder.Build() as Player.Player;
        sut.OnDamage(enemy, new CombatDamage(5, DamageType.Melee));

        Assert.Equal((uint)95, sut.HealthPoints);
    }

    [Fact]
    public void OnDamage_When_Receives_Mana_Attack_Reduce_Mana()
    {
        var sut = PlayerTestDataBuilder.Build(mana: 30) as Player.Player;
        var enemy = PlayerTestDataBuilder.Build() as Player.Player;
        sut.OnDamage(enemy, new CombatDamage(5, DamageType.ManaDrain));

        Assert.Equal((uint)25, sut.Mana);
    }

    [Fact]
    public void FlagIsEnabled_Enabled_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.SetFlag(PlayerFlag.IgnoreYellCheck);
        var result = sut.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);

        result.Should().BeTrue();
    }

    [Fact]
    public void FlagIsEnabled_Disabled_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        var result = sut.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);

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

    [Fact]
    public void ChangeChaseMode_Follow_InvokeFollow()
    {
        var sut = PlayerTestDataBuilder.Build(pathFinder: new Mock<IPathFinder>().Object);
        var enemy = PlayerTestDataBuilder.Build();

        var called = false;
        sut.OnStartedFollowing += (_, _, _) => { called = true; };

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

        sut.ChangeSecureMode(0);
        sut.SecureMode.Should().Be(0);
        sut.ChangeSecureMode(1);
        sut.SecureMode.Should().Be(1);
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
}