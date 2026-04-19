using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Extensions;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Server.Tests.Conditions;

public class PlayerConditionPersistenceTests
{
    [Fact]
    public void Player_conditions_round_trip_through_json_and_restore_runtime_effects()
    {
        // Arrange
        var sourcePlayer = (Player)PlayerTestDataBuilder.Build();
        var elapsedTicks = TimeSpan.FromSeconds(20).Ticks;

        var speedCondition = new ConditionSpeed(
            60000,
            new FormulaValues
            {
                FormulaType = FormulaType.Damage,
                MinA = 1,
                MinB = 50,
                MaxA = 1,
                MaxB = 51
            });
        sourcePlayer.AddCondition(speedCondition);
        var invisibleCondition = new ConditionInvisible(60000);
        sourcePlayer.AddCondition(invisibleCondition);

        var lightCondition = new ConditionLight(60000, 50, 215);
        sourcePlayer.AddCondition(lightCondition);

        var regenerationCondition = new Condition(ConditionType.Regeneration, 60000, sourcePlayer.SetAsHungry);
        sourcePlayer.AddCondition(regenerationCondition);

        SetStartedAt(speedCondition, DateTime.UtcNow.Ticks - elapsedTicks);
        SetStartedAt(invisibleCondition, DateTime.UtcNow.Ticks - elapsedTicks);
        SetStartedAt(lightCondition, DateTime.UtcNow.Ticks - elapsedTicks);
        SetStartedAt(regenerationCondition, DateTime.UtcNow.Ticks - elapsedTicks);

        var snapshot = JsonExtensions.SerializeConditions(sourcePlayer.GetConditions());
        using var document = JsonDocument.Parse(snapshot);
        var snapshots = document.RootElement.EnumerateArray().ToArray();

        JsonElement GetSnapshot(string runtimeType, ConditionType? conditionType = null)
        {
            return snapshots.Single(element =>
                element.GetProperty("RuntimeType").GetString() == runtimeType &&
                (!conditionType.HasValue || element.GetProperty("ConditionType").GetInt32() == (int)conditionType.Value));
        }

        var restoredConditions = JsonExtensions.DeserializeConditions(snapshot);
        var restoredSpeed = restoredConditions.OfType<ConditionSpeed>().Single();
        var restoredInvisible = restoredConditions.OfType<ConditionInvisible>().Single();
        var restoredLight = restoredConditions.OfType<ConditionLight>().Single();
        var restoredRegeneration = restoredConditions.OfType<Condition>().Single(condition => condition.Type == ConditionType.Regeneration);

        var speedDuration = GetSnapshot(typeof(ConditionSpeed).FullName!).GetProperty("Duration").GetInt64();
        var invisibleDuration = GetSnapshot(typeof(ConditionInvisible).FullName!).GetProperty("Duration").GetInt64();
        var lightDuration = GetSnapshot(typeof(ConditionLight).FullName!).GetProperty("Duration").GetInt64();
        var regenerationDuration = GetSnapshot(typeof(Condition).FullName!, ConditionType.Regeneration)
            .GetProperty("Duration").GetInt64();
        var expectedRemainingDuration = 60000 * TimeSpan.TicksPerMillisecond - elapsedTicks;
        var tolerance = TimeSpan.FromSeconds(2).Ticks;

        // Assert
        restoredSpeed.Type.Should().Be(ConditionType.Haste);
        restoredSpeed.Duration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        restoredSpeed.SpeedChange.Should().Be(50);
        restoredSpeed.IsDisabled.Should().BeFalse();

        restoredInvisible.Type.Should().Be(ConditionType.Invisible);
        restoredInvisible.Duration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        restoredInvisible.Effect.Should().Be(EffectT.None);

        restoredLight.Type.Should().Be(ConditionType.Light);
        restoredLight.Duration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        restoredLight.ColorLevel.Should().Be(50);
        restoredLight.Color.Should().Be(215);
        restoredLight.Effect.Should().Be(EffectT.None);

        restoredRegeneration.Type.Should().Be(ConditionType.Regeneration);
        restoredRegeneration.Duration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        restoredRegeneration.HasPersistentCounter.Should().BeFalse();

        speedDuration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        invisibleDuration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        lightDuration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
        regenerationDuration.Should().BeInRange(expectedRemainingDuration - tolerance, expectedRemainingDuration + tolerance);
    }

    [Fact]
    public void Player_load_conditions_adds_restored_conditions_to_the_player()
    {
        // Arrange
        var player = (Player)PlayerTestDataBuilder.Build();

        // Act
        player.LoadConditions([new Condition(ConditionType.LogoutBlock)]);

        // Assert
        player.HasCondition(ConditionType.LogoutBlock).Should().BeTrue();
        player.GetConditions().Should().ContainSingle(condition => condition.Type == ConditionType.LogoutBlock);
    }

    [Fact]
    public void Persistent_counter_is_not_restored_from_json()
    {
        // Arrange
        var condition = new Condition(ConditionType.Regeneration, 60000, () => { });
        condition.IncreasePersistentCounter();

        // Act
        var restored = JsonExtensions.DeserializeConditions(JsonExtensions.SerializeConditions([condition]));

        // Assert
        restored.Should().ContainSingle();
        restored.Single().HasPersistentCounter.Should().BeFalse();
    }

    private static void SetStartedAt(ICondition condition, long startedAt)
    {
        SetBackingField(condition, nameof(ICondition.StartedAt), startedAt);
        SetBackingField(condition, "EndTime", startedAt + condition.Duration);
    }

    private static void SetBackingField<T>(object target, string propertyName, T value)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var backingField = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField is null) continue;

            backingField.SetValue(target, value);
            return;
        }

        throw new InvalidOperationException($"Could not set backing field for '{propertyName}'.");
    }
}
