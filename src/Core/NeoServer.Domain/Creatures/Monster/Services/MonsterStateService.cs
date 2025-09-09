using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Creatures.Monster.Services;

/// <summary>
/// Service responsible for updating the state of a monster based on its current situation.
/// </summary>
public class MonsterStateService(ISummonService summonService, TargetDetectorService targetDetectorService)
{
    public void UpdateState(IMonster monster)
    {
        if (monster.IsDead) return;
        
        // Update the monster's targets before updating the state
        targetDetectorService.UpdateTargets(monster as Monster);

        // Update the monster's state based on its current situation
        monster.UpdateState();

        // Stop attacking if the current target is unreachable
        if (monster.IsCurrentTargetUnreachable)
        {
            monster.StopAttack();
        }
        
        if (monster.State == MonsterState.LookingForEnemy)
        {
            monster.LookForNewEnemy();
            monster.Summon(summonService);
        }

        if (monster.State == MonsterState.InCombat)
        {
            monster.MoveAroundEnemy();

            if (!monster.Attacking)
            {
                monster.SelectTargetToAttack();
                return;
            }

            monster.TurnTo(monster.CurrentTarget);
            monster.Follow(monster.CurrentTarget);

            monster.Summon(summonService);

            if (monster.Metadata.TargetChance.Interval == 0) return;

            if (monster.Attacking &&
                monster.Metadata.TargetChance.Chance < GameRandom.Random.Next(1, maxValue: 100)) return;

            monster.SelectTargetToAttack();
        }

        if (monster.State == MonsterState.Sleeping) monster.Sleep();
        if (monster.State == MonsterState.Escaping) monster.Escape();
    }
}