using NeoServer.Game.Combat.Services;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Results;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Events.Combat;

public class CreatureChangedAttackTargetEventHandler(IGameServer game, IPlayerSkullService skullService)
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

    private  Result AttackEnemy(ICombatActor actor, ICreature victim)
    {
        if (actor is IPlayer playerAggressor && victim is IPlayer playerEnemy)
        {
            skullService.UpdateSkullOnAttack(playerAggressor, playerEnemy);
        }
        
        return victim is not ICombatActor enemy ? Result.NotPossible : actor.Attack(enemy);
    }
}