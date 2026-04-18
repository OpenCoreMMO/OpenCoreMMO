using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Domain.Items.Services;

public static class ItemAbilityApplier
{
    public static Result ApplyAbilities(IPlayer player, IItem item)
    {
        if (Guard.AnyNull(player, item))
            throw new ArgumentException($"[{nameof(ItemAbilityApplier)}] Player or item cannot be null");

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.Speed, out var speed))
            player.IncreaseSpeed(speed);

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemTypeAttribute.Invisible, out var invisible) && invisible)
            player.TurnInvisible();

        if (item.Metadata.Attributes.TryGetAttribute<int>(ItemTypeAttribute.ManaShield, out var manaShield) &&
            manaShield == 1)
        {
            player.AddCondition(new Condition(ConditionType.ManaShield));
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.HealthGain, out var healthGain) &&
            healthGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.HealTicks, out var ticks);

            player.AddRegenerationBonus(new RegenerationBonus
            {
                Gain = healthGain,
                Ticks = ticks,
                Type = RegenerationType.Health
            });
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ManaGain, out var manaGain) &&
            manaGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ManaTicks, out var ticks);

            player.AddRegenerationBonus(new RegenerationBonus
            {
                Gain = manaGain,
                Ticks = ticks,
                Type = RegenerationType.Mana
            });
        }

        ToggleConditions(player, item, true);

        return Result.Success;
    }

    public static Result RemoveAbilities(IPlayer player, IItem item)
    {
        if (Guard.AnyNull(player, item))
            throw new ArgumentException($"[{nameof(ItemAbilityApplier)}] Player or item cannot be null");

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.Speed, out var speed))
            player.DecreaseSpeed(speed);

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemTypeAttribute.Invisible, out var invisible) && invisible)
            player.TurnVisible();

        if (item.Metadata.Attributes.TryGetAttribute<int>(ItemTypeAttribute.ManaShield, out var manaShield) &&
            manaShield == 1)
        {
            player.RemovePersistentCondition(ConditionType.ManaShield);
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.HealthGain, out var healthGain) &&
            healthGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.HealTicks, out var ticks);

            player.RemoveRegenerationBonus(new RegenerationBonus
            {
                Gain = healthGain,
                Ticks = ticks,
                Type = RegenerationType.Health
            });
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ManaGain, out var manaGain) &&
            manaGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ManaTicks, out var ticks);

            player.RemoveRegenerationBonus(new RegenerationBonus
            {
                Gain = manaGain,
                Ticks = ticks,
                Type = RegenerationType.Mana
            });
        }

        ToggleConditions(player, item, false);

        return Result.Success;
    }

    private static void ToggleConditions(IPlayer player, IItem item, bool suppress)
    {
        ReadOnlySpan<ItemTypeAttribute> suppressAttributes =
        [
            ItemTypeAttribute.SuppressDrown, ItemTypeAttribute.SuppressDrunk,
            ItemTypeAttribute.SuppressCurse, ItemTypeAttribute.SuppressDazzle,
            ItemTypeAttribute.SuppressEnergy, ItemTypeAttribute.SuppressFire,
            ItemTypeAttribute.SuppressFreeze, ItemTypeAttribute.SuppressPoison
        ];

        foreach (var suppressAttribute in suppressAttributes)
        {
            if (!item.Metadata.Attributes.TryGetAttribute<bool>(suppressAttribute, out var suppressCondition)) continue;
            if (!suppressCondition) continue;

            var condition = suppressAttribute switch
            {
                ItemTypeAttribute.SuppressDrown => ConditionType.Drowning,
                ItemTypeAttribute.SuppressDrunk => ConditionType.Drunk,
                ItemTypeAttribute.SuppressCurse => ConditionType.Cursed,
                ItemTypeAttribute.SuppressDazzle => ConditionType.Dazzled,
                ItemTypeAttribute.SuppressEnergy => ConditionType.Electrified,
                ItemTypeAttribute.SuppressFire => ConditionType.Burning,
                ItemTypeAttribute.SuppressFreeze => ConditionType.Freezing,
                ItemTypeAttribute.SuppressPoison => ConditionType.Poisoned,
                _ => ConditionType.None
            };

            if (condition == ConditionType.None) continue;

            if (suppress)
            {
                player.AddConditionSuppression(condition);

                if (player.GetConditionSuppressionCount(condition) > 1)
                    return;

                player.DisableCondition(condition);
                return;
            }

            player.RemoveConditionSuppression(condition);

            if (player.GetConditionSuppressionCount(condition) > 0)
                return;

            player.EnableCondition(condition);
        }
    }
}