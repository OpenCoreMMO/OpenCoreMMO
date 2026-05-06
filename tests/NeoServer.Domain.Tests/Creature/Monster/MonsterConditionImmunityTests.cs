using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterConditionImmunityTests
{
    [Fact]
    public void Monster_rejects_paralyze_when_immune_to_paralysis()
    {
        var monster = MonsterTestDataBuilder.Build();
        ((MonsterType)monster.Metadata).Immunities = (ushort)Immunity.Paralysis;

        monster.AddCondition(new ParalyzeCondition(10_000, 200));

        monster.HasCondition(ConditionType.Paralyze).Should().BeFalse();
    }

    [Fact]
    public void Monster_rejects_burning_when_immune_to_fire()
    {
        var monster = MonsterTestDataBuilder.Build();
        ((MonsterType)monster.Metadata).Immunities = (ushort)Immunity.Fire;

        monster.AddCondition(new Condition(ConditionType.Burning, 1000));

        monster.HasCondition(ConditionType.Burning).Should().BeFalse();
    }

    [Fact]
    public void Monster_rejects_poisoned_when_immune_to_earth()
    {
        var monster = MonsterTestDataBuilder.Build();
        ((MonsterType)monster.Metadata).Immunities = (ushort)Immunity.Earth;

        monster.AddCondition(new Condition(ConditionType.Poisoned, 1000));

        monster.HasCondition(ConditionType.Poisoned).Should().BeFalse();
    }

    [Fact]
    public void Monster_rejects_drunk_when_immune_to_drunkenness()
    {
        var monster = MonsterTestDataBuilder.Build();
        ((MonsterType)monster.Metadata).Immunities = (ushort)Immunity.Drunkenness;

        monster.AddCondition(new Condition(ConditionType.Drunk, 1000));

        monster.HasCondition(ConditionType.Drunk).Should().BeFalse();
    }

    [Fact]
    public void Monster_accepts_paralyze_when_not_immune()
    {
        var monster = MonsterTestDataBuilder.Build();

        monster.AddCondition(new ParalyzeCondition(10_000, 200));

        monster.HasCondition(ConditionType.Paralyze).Should().BeTrue();
    }

    [Fact]
    public void Monster_accepts_condition_when_immune_to_different_type()
    {
        var monster = MonsterTestDataBuilder.Build();
        ((MonsterType)monster.Metadata).Immunities = (ushort)Immunity.Fire;

        monster.AddCondition(new ParalyzeCondition(10_000, 200));

        monster.HasCondition(ConditionType.Paralyze).Should().BeTrue();
    }

    [Fact]
    public void Monster_dismiss_clears_conditions()
    {
        var monster = MonsterTestDataBuilder.Build();
        monster.AddCondition(new Condition(ConditionType.Burning, 1000));
        monster.HasCondition(ConditionType.Burning).Should().BeTrue();

        ((NeoServer.Domain.Creatures.Monster.Monster)monster).Dismiss();

        monster.HasCondition(ConditionType.Burning).Should().BeFalse();
    }
}
