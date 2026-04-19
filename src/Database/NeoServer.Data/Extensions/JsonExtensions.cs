using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Extensions;

public static class JsonExtensions
{
    public static string SerializeAttributes<T>(Dictionary<T, string> dict) where T : Enum
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<T, string> DeserializeAttributes<T>(string json) where T : Enum
    {
        return JsonSerializer.Deserialize<Dictionary<T, string>>(json);
    }

    public static string SerializeCustomAttributes(Dictionary<string, string> dict)
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<string, string> DeserializeCustomAttributes(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    public static string SerializeAllAttributes(Dictionary<string, string> dict)
    {
        return JsonSerializer.Serialize(dict);
    }

    public static Dictionary<string, string> DeserializeAllAttributes(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    public static string SerializeConditions(IEnumerable<ICondition> conditions)
    {
        var snapshots = conditions?
            .Where(condition => condition is not null && !condition.IsPersistent)
            .Select(CreateSnapshot)
            .ToArray();

        if (snapshots is null || snapshots.Length == 0) return null;

        return JsonSerializer.Serialize(snapshots);
    }

    public static List<ICondition> DeserializeConditions(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        var snapshots = JsonSerializer.Deserialize<List<ConditionSnapshot>>(json);
        if (snapshots is null || snapshots.Count == 0) return [];

        var conditions = new List<ICondition>(snapshots.Count);

        foreach (var snapshot in snapshots)
        {
            var condition = RestoreCondition(snapshot);
            if (condition is null) continue;

            conditions.Add(condition);
        }

        return conditions;
    }

    private static ConditionSnapshot CreateSnapshot(ICondition condition)
    {
        var remainingDuration = Math.Max(0, condition.StartedAt + condition.Duration - DateTime.UtcNow.Ticks);

        return condition switch
        {
            ConditionSpeed speed => new ConditionSnapshot(
                speed.GetType().FullName,
                speed.Type,
                remainingDuration,
                speed.IsDisabled,
                speed.FormulaValues,
                new Dictionary<ConditionParamType, uint>(speed.Parameters),
                JsonSerializer.SerializeToElement(new ConditionSpeedState(
                    speed.Interval,
                    speed.Effect,
                    speed.SpeedChange))),
            ConditionLight light => new ConditionSnapshot(
                light.GetType().FullName,
                light.Type,
                remainingDuration,
                light.IsDisabled,
                light.FormulaValues,
                new Dictionary<ConditionParamType, uint>(light.Parameters),
                JsonSerializer.SerializeToElement(new ConditionLightState(
                    light.Duration == 0 ? 0 : (uint)(light.Duration / TimeSpan.TicksPerMillisecond),
                    light.ColorLevel,
                    light.Color,
                    light.LightChangeInterval,
                    light.CurrentColorLevel,
                    light.Effect))),
            ConditionInvisible invisible => new ConditionSnapshot(
                invisible.GetType().FullName,
                invisible.Type,
                remainingDuration,
                invisible.IsDisabled,
                invisible.FormulaValues,
                new Dictionary<ConditionParamType, uint>(invisible.Parameters),
                JsonSerializer.SerializeToElement(new ConditionInvisibleState(
                    invisible.Duration == 0 ? 0 : (uint)(invisible.Duration / TimeSpan.TicksPerMillisecond),
                    invisible.Effect))),
            ConditionDamage damage => new ConditionSnapshot(
                damage.GetType().FullName,
                damage.Type,
                remainingDuration,
                damage.IsDisabled,
                damage.FormulaValues,
                new Dictionary<ConditionParamType, uint>(damage.Parameters),
                JsonSerializer.SerializeToElement(CreateDamageState(damage))),
            Condition genericCondition => new ConditionSnapshot(
                genericCondition.GetType().FullName,
                genericCondition.Type,
                remainingDuration,
                genericCondition.IsDisabled,
                genericCondition.FormulaValues,
                new Dictionary<ConditionParamType, uint>(genericCondition.Parameters),
                JsonSerializer.SerializeToElement(new GenericConditionState())),
            _ => throw new NotSupportedException($"Unsupported condition type '{condition.GetType().FullName}'.")
        };
    }

    private static ConditionDamageState CreateDamageState(ConditionDamage damage)
    {
        var cooldown = GetFieldValue<CooldownTime>(damage, "_cooldown");
        var damageQueue = GetFieldValue<Queue<ushort>>(damage, "_damageQueue") ?? [];
        var minDamage = GetFieldValue<ushort>(damage, "_minDamage");
        var maxDamage = GetFieldValue<ushort>(damage, "_maxDamage");

        return new ConditionDamageState(
            cooldown.Duration == 0 ? 0 : (uint)(cooldown.Duration / TimeSpan.TicksPerMillisecond),
            damage.Amount,
            minDamage,
            maxDamage,
            damage.DamageType,
            damage.Effect,
            [.. damageQueue],
            cooldown.Start,
            cooldown.Duration);
    }

    private static ICondition RestoreCondition(ConditionSnapshot snapshot)
    {
        var condition = snapshot.RuntimeType switch
        {
            var runtimeType when runtimeType == typeof(ConditionSpeed).FullName =>
                RestoreConditionSpeed(snapshot),
            var runtimeType when runtimeType == typeof(ConditionLight).FullName =>
                RestoreConditionLight(snapshot),
            var runtimeType when runtimeType == typeof(ConditionInvisible).FullName =>
                RestoreConditionInvisible(snapshot),
            var runtimeType when runtimeType == typeof(ConditionDamage).FullName =>
                RestoreConditionDamage(snapshot),
            var runtimeType when runtimeType == typeof(Condition).FullName =>
                RestoreGenericCondition(snapshot),
            _ => null
        };

        if (condition is null) return null;

        RestoreBaseState(condition, snapshot);

        return condition;
    }

    private static ICondition RestoreGenericCondition(ConditionSnapshot snapshot)
    {
        var duration = (uint)(snapshot.Duration / TimeSpan.TicksPerMillisecond);
        return duration == 0
            ? new Condition(snapshot.ConditionType)
            : new Condition(snapshot.ConditionType, duration);
    }

    private static ICondition RestoreConditionSpeed(ConditionSnapshot snapshot)
    {
        var state = snapshot.State.Deserialize<ConditionSpeedState>();
        if (state is null) return null;

        var condition = new ConditionSpeed(state.Interval, snapshot.FormulaValues, state.Effect);
        SetPropertyValue(condition, nameof(ConditionSpeed.SpeedChange), state.SpeedChange);
        return condition;
    }

    private static ICondition RestoreConditionLight(ConditionSnapshot snapshot)
    {
        var state = snapshot.State.Deserialize<ConditionLightState>();
        if (state is null) return null;

        var condition = new ConditionLight(state.Interval, state.CurrentColorLevel, state.Color, state.Effect);
        SetPropertyValue(condition, nameof(ConditionLight.LightChangeInterval), state.LightChangeInterval);
       
        return condition;
    }

    private static ICondition RestoreConditionInvisible(ConditionSnapshot snapshot)
    {
        var state = snapshot.State.Deserialize<ConditionInvisibleState>();
        return state is null ? null : new ConditionInvisible(state.Interval, state.Effect);
    }

    private static ICondition RestoreConditionDamage(ConditionSnapshot snapshot)
    {
        var state = snapshot.State.Deserialize<ConditionDamageState>();
        if (state is null) return null;

        if (state.DamageQueue.Count == 0 && state.Amount == 0) return null;

        var condition = new ConditionDamage(null, snapshot.ConditionType, state.Interval, state.MinDamage, state.MaxDamage,
            state.Effect)
        {
            DamageType = state.DamageType
        };

        SetFieldValue(condition, "_cooldown", new CooldownTime
        {
            Start = state.CooldownStart,
            Duration = state.CooldownDuration
        });
        SetFieldValue(condition, "_damageQueue", new Queue<ushort>(state.DamageQueue));

        return condition;
    }

    private static void RestoreBaseState(ICondition condition, ConditionSnapshot snapshot)
    {
        SetPropertyValue(condition, nameof(ICondition.FormulaValues), snapshot.FormulaValues);
        SetPropertyValue(condition, nameof(ICondition.Parameters), snapshot.Parameters ?? new Dictionary<ConditionParamType, uint>());
        SetPropertyValue(condition, nameof(ICondition.IsDisabled), snapshot.IsDisabled);
        SetPropertyValue(condition, nameof(ICondition.Duration), snapshot.Duration);
    }

    private static void SetPropertyValue<T>(object target, string propertyName, T value)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var backingField = type.GetField($"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingField is not null)
            {
                backingField.SetValue(target, value);
                return;
            }
        }

        var property = target.GetType().GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property?.GetSetMethod(true)?.Invoke(target, [value]);
    }

    private static void SetFieldValue<T>(object target, string fieldName, T value)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) continue;

            field.SetValue(target, value);
            return;
        }
    }

    private static T GetFieldValue<T>(object target, string fieldName)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) continue;

            return (T)field.GetValue(target);
        }

        return default;
    }

    private sealed record ConditionSnapshot(
        string RuntimeType,
        ConditionType ConditionType,
        long Duration,
        bool IsDisabled,
        FormulaValues FormulaValues,
        Dictionary<ConditionParamType, uint> Parameters,
        JsonElement State);

    private sealed record GenericConditionState;

    private sealed record ConditionSpeedState(
        uint Interval,
        EffectT Effect,
        ushort SpeedChange);

    private sealed record ConditionLightState(
        uint Interval,
        uint ColorLevel,
        uint Color,
        uint LightChangeInterval,
        byte CurrentColorLevel,
        EffectT Effect);

    private sealed record ConditionInvisibleState(
        uint Interval,
        EffectT Effect);

    private sealed record ConditionDamageState(
        uint Interval,
        byte Amount,
        ushort MinDamage,
        ushort MaxDamage,
        DamageType DamageType,
        EffectT Effect,
        List<ushort> DamageQueue,
        long CooldownStart,
        long CooldownDuration);
}
