using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Monster;
using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Events.Combat;

public class CreatureChangedAttackTargetEventHandler(
    IGameServer game,
    MonsterCombatService monsterCombatService,
    PlayerCombatService playerCombatService)
{
    public void Execute(ICombatActor actor, uint oldTarget, uint newTarget)
    {
        if (actor.AttackEvent != 0) return;

        var result = Attack(actor);
        var attackSpeed = result ? actor.AttackSpeed : 300;
        actor.AttackEvent = game.Scheduler.AddEvent(new SchedulerEvent((int)attackSpeed, () => Attack(actor)));
    }

    private bool Attack(ICombatActor actor)
    {
        var result = Result.NotPossible;

        if (actor.Attacking)
        {
            game.CreatureManager.TryGetCreature(actor.AutoAttackTargetId, out var creature);

            result = AttackEnemy(actor, creature);
        }
        else
        {
            if (actor.AttackEvent != 0)
            {
                game.Scheduler.CancelEvent(actor.AttackEvent);
                actor.AttackEvent = 0;
            }
        }

        if (actor.AttackEvent == 0) return result.Succeeded;

        actor.AttackEvent = 0;
        Execute(actor, 0, 0);

        return result.Succeeded;
    }

    private Result AttackEnemy(ICombatActor actor, ICreature victim)
    {
        // if (actor is IPlayer playerAggressor && victim is IPlayer playerEnemy)
        //     skullService.UpdateSkullOnAttack(playerAggressor, playerEnemy);

        if (victim is not ICombatActor target) return Result.NotPossible;

        switch (actor)
        {
            case IMonster monster:
                monsterCombatService.Attack(monster, target);
                break;
            case IPlayer player:
                playerCombatService.Attack(player, target);
                break;
        }

        return Result.Success;
    }
}