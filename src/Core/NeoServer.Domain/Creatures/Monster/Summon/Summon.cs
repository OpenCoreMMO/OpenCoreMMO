using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Monster.Summon;

public class Summon : Monster
{
    public Summon(IMonsterType type, IMapTool mapTool, ICreature master) : base(type, mapTool, null)
    {
        Master = master;
        if (master is not null)
        {
            Master.Summons.Add(this);
        }
    }

    public override bool IsSummon => true;

    public ICreature Master { get; }

    public override FindPathParams PathSearchParams
    {
        get
        {
            var fpp = base.PathSearchParams;
            fpp.MinTargetDist = 1;
            fpp.MaxTargetDist = Equals(FollowCreature, Master) ? 2 : TargetDistance;
            fpp.FullPathSearch = true;
            fpp.KeepDistance = false;
            fpp.ClearSight = true;
            return fpp;
        }
    }

    public override void SetAsEnemy(ICreature creature)
    {
        if (IsDead) return;
        if (Master is not null && Master.Equals(creature)) return;
        if (creature.Equals(this)) return;
        if (creature is Summon { Master: not null } summon && summon.Master.Equals(Master)) return;

        //Summon should not attack if the master has no target
        if (Master is ICombatActor { CurrentTarget: null }) return;

        if (!CanSee(creature.Location)) return;

        base.SetAsEnemy(creature);
    }

    public override Result SetAttackTarget(ICreature target)
    {
        if (IsDead) return Result.NotPossible;
        if (Master is not null && Master.Equals(target)) return Result.NotPossible;
        if (target is Summon { Master: not null } summon && summon.Master.Equals(Master)) return Result.NotPossible;
        if (target.Equals(this)) return Result.NotPossible;

        //Summon should not attack if the master has no target
        if (Master is ICombatActor { CurrentTarget: null }) return Result.NotPossible;

        if (!CanSee(target.Location)) return Result.NotPossible;

        return base.SetAttackTarget(target);
    }

    public override void Born(Location location)
    {
        base.Born(location);
        Awake();

        if (Master is ICombatActor { CurrentTarget: not null } combatMaster)
        {
            SetAsEnemy(combatMaster.CurrentTarget);
            ChangeAttackTarget(combatMaster.CurrentTarget);
        }
    }

    public override void UpdateState()
    {
        // Check if summon should disappear due to distance or floor change
        if (Master is ICombatActor { IsDead: false })
        {
            var floorDifference = Math.Abs(Master.Location.Z - Location.Z);
            var distance = Master.Location.GetSqmDistance(Location);

            if (floorDifference >= 2 || distance > 40)
            {
                Die();
                return;
            }
        }

        if (!CanSee(Master.Location))
        {
            Targets.Clear();
            State = MonsterState.RandomlyWalking;
            return;
        }

        if (Master is IMonster { State: MonsterState.RandomlyWalking })
        {
            State = MonsterState.Awake;
            return;
        }

        if (CanSee(Master.Location) && State is MonsterState.RandomlyWalking) State = MonsterState.Awake;

        if (Master is ICombatActor { CurrentTarget: not null } combatMaster)
        {
            ChangeAttackTarget(combatMaster.CurrentTarget);
            return;
        }

        Follow(Master);
    }

    public override void Dismiss()
    {
        if (Master is not null)
        {
            Master.Summons.Remove(this);
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

    public void OnMasterKilled()
    {
        Die();
    }

    public void OnMasterLogout()
    {
        Die();
    }

    public override bool CanSee(Location pos, int viewRangeX, int viewRangeY, int limitRangeOffset = 0)
    {
        if (base.CanSee(pos, viewRangeX, viewRangeY, limitRangeOffset)) return true;

        if (Master is null) return false;

        //summon should see what the master can see as long he can see the master
        return Master.CanSee(pos, viewRangeX, viewRangeY, limitRangeOffset) &&
               base.CanSee(Master.Location, viewRangeX, viewRangeY, limitRangeOffset);
    }


    public override bool CanSee(Location location)
    {
        if (base.CanSee(location)) return true;

        if (Master is null) return false;

        //summon should see what the master can see as long he can see the master
        return Master.CanSee(location) && base.CanSee(Master.Location);
    }

    public void OnMasterChangeTarget(ICombatActor master)
    {
        Targets.Clear();

        if (!CanSee(master.CurrentTarget?.Location ?? Location.Zero)) return;

        SetAsEnemy(master.CurrentTarget);
        ChangeAttackTarget(master.CurrentTarget);
    }

}