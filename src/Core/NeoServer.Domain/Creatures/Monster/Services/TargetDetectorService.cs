using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Creatures.Monster.Services;

public class TargetDetectorService(IMap map)
{
    /// <summary>
    /// Maintains the monster's combat focus by cleaning up its target list.
    /// Removes targets that are no longer viable threats, such as dead creatures or those outside the monster's perception range.
    /// This ensures the monster only pursues active, reachable enemies.
    /// </summary>
    /// <param name="monster">The monster whose target priorities need updating.</param>
    public void Update(Monster monster)
    {
        // Begin checking from the monster's first tracked target
        var current = monster.Targets.First;

        // Review each target in the monster's list
        while (current != null)
        {
            var target = current.Value;

            // Determine if this target is still a valid threat
            if (target.IsDead || !monster.CanSee(target) || !monster.CanSee(target.Location))
            {
                var next = current.Next; // Remember the next target before removing this one
                monster.Targets.Remove(target); // Remove the invalid target from tracking
                current = next;
                continue; // Proceed to check the next target
            }

            current = current.Next;
        }

        // Scan nearby creatures to find new potential targets
        var spectators = map.GetSpectators(monster.Location);

        foreach (var spectator in spectators)
        {
            // Only consider players and their summons as valid targets
            if (spectator is not ICombatActor target) continue;
            if (ReferenceEquals(spectator, monster)) continue;

            // Skip players that are flagged as not being attackable
            if (target is IPlayer player && player.Group.FlagIsEnabled(PlayerFlag.IgnoredByMonsters))
                continue;

            var isPlayerOrPlayerSummon = spectator is IPlayer or Summon.Summon { Master: IPlayer };

            if (!isPlayerOrPlayerSummon) 
                continue;

            // Skip dead creatures
            if (target.IsDead) continue;

            // Must be visible to the monster
            if (!monster.CanSee(target.Location)) continue;
            if (!monster.CanSee(target)) continue;
            
            // Must be on the same floor
            if (!monster.Location.SameFloorAs(target.Location)) continue;

            // Add as a new target (MonsterTargetList handles duplicates)
            monster.Targets.Add(target, false);
        }
    }
}