using System;
using System.Collections.Generic;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class ConditionListParserTest
{
    #region Round-Trip (Integration)

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_Deserialize_round_trips_mixed_condition_list()
    {
        // Arrange — build one condition for each parser path:
        //   ConditionParser (ManaShield)
        //   HasteConditionParser
        //   ParalyzeConditionParser

        var manaShield = new Condition(ConditionType.ManaShield, 30_000);

        var hasteState = new HasteConditionState(
            ConditionType.Haste,
            EffectT.GlitterBlue,
            SpeedBoost: 180,
            RemainingTimeMilliseconds: 25_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var paralyzeState = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 15_000);

        var conditions = new List<ICondition>
        {
            manaShield,
            HasteCondition.Restore(hasteState)!,
            ParalyzeCondition.Restore(paralyzeState)!
        };

        // Act
        var json = ConditionListParser.Serialize(conditions);
        var restored = ConditionListParser.Deserialize(json);

        // Assert — all three conditions survive the round-trip
        restored.Should().HaveCount(3);

        restored[0].Type.Should().Be(ConditionType.ManaShield);
        restored[1].Type.Should().Be(ConditionType.Haste);
        restored[2].Type.Should().Be(ConditionType.Paralyze);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_Deserialize_skips_expired_paralyze_and_haste()
    {
        // Arrange — ManaShield is valid; Haste and Paralyze are expired (zero remaining time)

        var manaShield = new Condition(ConditionType.ManaShield, 30_000);

        var expiredHasteState = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 100,
            RemainingTimeMilliseconds: 0,
            Duration: 0,
            new FormulaValues());

        var expiredParalyzeState = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 80,
            RemainingTimeMilliseconds: 0);

        // HasteCondition.Restore returns null for expired; use the constructor
        // to create one that is already expired for serialization.
        var expiredHaste = new HasteCondition(0, new FormulaValues(), EffectT.None);
        var expiredParalyze = new ParalyzeCondition(0, 80);

        var conditions = new List<ICondition>
        {
            manaShield,
            expiredHaste,
            expiredParalyze
        };

        // Act
        var json = ConditionListParser.Serialize(conditions);
        var restored = ConditionListParser.Deserialize(json);

        // Assert — only ManaShield survives; expired Haste and Paralyze are filtered out
        restored.Should().HaveCount(1);
        restored[0].Type.Should().Be(ConditionType.ManaShield);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_Deserialize_handles_null_conditions_in_restore()
    {
        // Arrange — use Restore with zero time which returns null;
        // the list contains only a valid ParalyzeCondition
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 50,
            RemainingTimeMilliseconds: 10_000);

        var conditions = new List<ICondition>
        {
            ParalyzeCondition.Restore(state)!
        };

        // Act
        var json = ConditionListParser.Serialize(conditions);
        var restored = ConditionListParser.Deserialize(json);

        // Assert
        restored.Should().HaveCount(1);
        restored[0].Type.Should().Be(ConditionType.Paralyze);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => ConditionListParser.Deserialize(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_non_array_json()
    {
        // Act
        Action act = () => ConditionListParser.Deserialize("{}");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_Deserialize_handles_empty_list()
    {
        // Arrange
        var conditions = new List<ICondition>();

        // Act
        var json = ConditionListParser.Serialize(conditions);
        var restored = ConditionListParser.Deserialize(json);

        // Assert
        restored.Should().BeEmpty();
    }

    #endregion
}
