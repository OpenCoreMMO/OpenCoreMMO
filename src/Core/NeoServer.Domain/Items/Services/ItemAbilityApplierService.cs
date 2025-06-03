using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Items.Services;

public class ItemAbilityApplierService : IItemAbilityApplierService
{
    public Result ApplyAbilities(IPlayer player, IItem item)
    {
        if (Guard.AnyNull(player, item))
            throw new ArgumentException($"[{nameof(ItemAbilityApplierService)}] Player or item cannot be null");

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.Speed, out var speed))
            player.IncreaseSpeed(speed);

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemAttribute.Invisible, out var invisible) && invisible)
            player.TurnInvisible();

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemAttribute.ManaShield, out var manaShield) && manaShield)
            player.EnableManaShield();

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.HealthGain, out var healthGain) &&
            healthGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.HealTicks, out var ticks);

            player.AddRegenerationBonus(new RegenerationBonus
            {
                Gain = healthGain,
                Ticks = ticks,
                Type = RegenerationType.Health
            });
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.ManaGain, out var manaGain) && manaGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.ManaTicks, out var ticks);

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

    public Result RemoveAbilities(IPlayer player, IItem item)
    {
        if (Guard.AnyNull(player, item))
            throw new ArgumentException($"[{nameof(ItemAbilityApplierService)}] Player or item cannot be null");

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.Speed, out var speed))
            player.DecreaseSpeed(speed);

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemAttribute.Invisible, out var invisible) && invisible)
            player.TurnVisible();

        if (item.Metadata.Attributes.TryGetAttribute<bool>(ItemAttribute.ManaShield, out var manaShield) && manaShield)
            player.DisableManaShield();

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.HealthGain, out var healthGain) &&
            healthGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.HealTicks, out var ticks);

            player.RemoveRegenerationBonus(new RegenerationBonus
            {
                Gain = healthGain,
                Ticks = ticks,
                Type = RegenerationType.Health
            });
        }

        if (item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.ManaGain, out var manaGain) && manaGain > 0)
        {
            item.Metadata.Attributes.TryGetAttribute<ushort>(ItemAttribute.ManaTicks, out var ticks);

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

    private static void ToggleConditions(IPlayer player, IItem item, bool supress)
    {
        ReadOnlySpan<ItemAttribute> suppressAttributes =
        [
            ItemAttribute.SuppressDrown, ItemAttribute.SuppressDrunk,
            ItemAttribute.SuppressCurse, ItemAttribute.SuppressDazzle,
            ItemAttribute.SuppressEnergy, ItemAttribute.SuppressFire,
            ItemAttribute.SuppressFreeze, ItemAttribute.SuppressPhysical,
            ItemAttribute.SuppressPoison
        ];

        foreach (var suppressAttribute in suppressAttributes)
        {
            if (!item.Metadata.Attributes.TryGetAttribute<bool>(suppressAttribute, out var suppress)) continue;
            if (!suppress) continue;

            var condition = suppressAttribute switch
            {
                ItemAttribute.SuppressDrown => ConditionType.Drown,
                ItemAttribute.SuppressDrunk => ConditionType.Drunk,
                ItemAttribute.SuppressCurse => ConditionType.Cursed,
                ItemAttribute.SuppressDazzle => ConditionType.Dazzled,
                ItemAttribute.SuppressEnergy => ConditionType.Energy,
                ItemAttribute.SuppressFire => ConditionType.Fire,
                ItemAttribute.SuppressFreeze => ConditionType.Freezing,
                ItemAttribute.SuppressPhysical => ConditionType.None,
                ItemAttribute.SuppressPoison => ConditionType.Poison,
                _ => ConditionType.None
            };

            if (condition == ConditionType.None) continue;

            if (supress)
            {
                player.DisableCondition(condition);
                return;
            }

            player.EnableCondition(condition);
        }
    }
}