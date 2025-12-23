using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;

namespace NeoServer.Domain.Creatures.Monster.Services;

/// <summary>
///     Service responsible for updating the state of a monster based on its current situation.
/// </summary>
public class MonsterStateService(
    ISummonService summonService,
    TargetDetectorService targetDetectorService,
    IMonsterTargetingService targetingService)
{
    public void UpdateState(IMonster monster)
    {
        if (monster.IsDead) return;
        var monsterEntity = monster as Monster;

        // Update the monster's targets before updating the state
        //targetDetectorService.UpdateTargets(monster as Monster);

        targetDetectorService.Update(monsterEntity);

        // Set a new target if the monster is not currently targeting one
        targetingService.SelectTarget(monsterEntity);

        // Update the monster's state based on its current situation
        monster.UpdateState();

        // If there are no targets, stop following and attacking
        if (!monster.Targets.Any() && monster is not Summon.Summon)
        {
            monster.StopAttack();
            monster.StopFollowing();
        }

        if (monster.State == MonsterState.RandomlyWalking)
        {
            //Walk a random step
            monster.DoRandomStep();
            monster.CreateSummon(summonService);
        }

        if (monster.State == MonsterState.InCombat)
        {
            monster.MoveAroundEnemy();

            if (!monster.Attacking)
            {
                targetingService.SelectTarget(monsterEntity);
                return;
            }

            monster.TurnTo(monster.CurrentTarget);
            monster.Follow(monster.CurrentTarget);

            monster.CreateSummon(summonService);
        }

        if (monster.State == MonsterState.Escaping) monster.Escape();

        if (monster.State == MonsterState.Sleeping) monster.Sleep();
    }
}