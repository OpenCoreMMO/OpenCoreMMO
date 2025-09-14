using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Monster.Summon;

public class Summon : Monster
{
    public Summon(IMonsterType type, IMapTool mapTool, ICreature master) : base(type, mapTool, null)
    {
        Master = master;
        if (master is not null)
        {
            Master.Summons.Add(this);

            if (master is ICombatActor actor)
            {
                actor.OnTargetChanged += OnMasterTargetChange;
                actor.OnStoppedAttack += OnMasterStoppedAttack;
            }

            if (master is IPlayer player)
            {
                player.OnLoggedOut += OnMasterLoggedOut;
            }
        }
    }

    public override bool IsSummon => true;

    public ICreature Master { get; }

    public override void SetAsEnemy(ICreature creature)
    {
        if (IsDead) return;
        if (Master is not null && Master.Equals(creature)) return;
        if (creature is Summon summon && summon.Master is not null && summon.Master.Equals(Master)) return;

        base.SetAsEnemy(creature);
    }

    public override void Born(Location location)
    {
        base.Born(location);
        Awake();
    }

    public override void UpdateState()
    {
        if (Master is not IPlayer player)
        {
            base.UpdateState();
            return;
        }

        if (player.CurrentTarget is not null)
        {
            ChangeAttackTarget(player.CurrentTarget);
            return;
        }

        Follow(Master);
    }

    public override void Dismiss()
    {
        if (Master is not null)
        {
            Master.Summons.Remove(this);

            if (Master is ICombatActor actor)
            {
                actor.OnTargetChanged -= OnMasterTargetChange;
                actor.OnStoppedAttack -= OnMasterStoppedAttack;
            }

            if (Master is IPlayer player)
            {
                player.OnLoggedOut -= OnMasterLoggedOut;
            }
        }

        base.Dismiss();
    }

    public override bool IsHostileTo(ICombatActor enemy)
    {
        if (Master is IPlayer player)
        {
            if (enemy.Equals(this)) return false;
            if (enemy.Equals(player)) return false;

            return true; // TODO: Check PvP
        }


        return base.IsHostileTo(enemy);
    }

    public override void Death(IThing by)
    {
        base.Death(by);

        if (Master is not null)
        {
            Master.OnSummonDie(this);
            Master.Summons.Remove(this);
        }

        Dismiss();
    }

    private void Die()
    {
        HealthPoints = 0;
        Death(this);
    }


    public void OnMasterKilled()
    {
        Die();
    }

    private void OnMasterTargetChange(ICombatActor actor, uint oldTargetId, uint newTargetId)
    {
        Targets.Clear();

        SetAsEnemy(actor.CurrentTarget);
        ChangeAttackTarget(actor.CurrentTarget);
    }

    private void OnMasterStoppedAttack(ICombatActor actor)
    {
        StopAttack();
    }


    private void OnMasterLoggedOut(IPlayer player)
    {
        Die();
    }
}