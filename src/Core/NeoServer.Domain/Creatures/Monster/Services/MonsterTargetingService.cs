using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Monster.Combat;

namespace NeoServer.Domain.Creatures.Monster.Services;

public interface IMonsterTargetingService
{
    void SelectTarget(Monster monster);
}

/// <summary>
/// Applies targeting policies for a monster, deciding when to switch or acquire targets using the search strategies.
/// </summary>
public class MonsterTargetingService(IMonsterTargetSearch targetSearch) : IMonsterTargetingService
{
    public void SelectTarget(Monster monster)
    {
        if (monster is null) return;

        var hasTargetChange = monster.Metadata.TargetChance.Chance > 0;

        if (monster.Attacking && monster.HasFollowPath && !hasTargetChange) return;

        var searchMode = TargetSearchType.Default;

        if (hasTargetChange)
        {
            var targetDistance = GetTargetDistance(monster);
            searchMode = targetDistance <= 1 ? TargetSearchType.Random : TargetSearchType.Nearest;
        }

        var candidate = targetSearch.Search(monster, searchMode);

        if (!monster.Attacking || !monster.HasFollowPath)
        {
            monster.ChangeAttackTarget(candidate);
            return;
        }

        var shouldChangeTarget = hasTargetChange &&
                                 monster.Cooldowns.Cooldowns.TryGetValue(CooldownType.TargetChange,
                                     out var cooldown) &&
                                 cooldown.Expired &&
                                 monster.Metadata.TargetChance.Chance >=
                                 GameRandom.Random.Next(1, maxValue: 100);

        if (shouldChangeTarget)
        {
            monster.ChangeAttackTarget(candidate);
        }
    }

    private static byte GetTargetDistance(Monster monster)
    {
        if (monster.Metadata.Flags.TryGetValue(CreatureFlagAttribute.TargetDistance, out var value))
        {
            return (byte)value;
        }

        return 1;
    }
}
