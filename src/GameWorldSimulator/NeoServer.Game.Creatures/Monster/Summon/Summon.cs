using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Creatures.Monster.Summon;

public class Summon : Monster
{
    public Summon(IMonsterType type, IMapTool mapTool, ICreature master) : base(type, mapTool, null)
    {
        Master = master;
        Master.Summons.Add(this);

        if (master is not ICombatActor actor) return;
        actor.OnDeath += OnMasterKilled;
        actor.OnTargetChanged += OnMasterTargetChange;
        actor.OnStoppedAttack += OnMasterStoppedAttack;

        if (master is not IPlayer player) return;
        player.OnLoggedOut += OnMasterLoggedOut;
    }

    public ICreature Master { get; }
    public override bool IsSummon => true;

    public override void SetAsEnemy(ICreature creature)
    {
        if (IsDead) return;
        if (Master.Equals(creature)) return;
        if (creature is Summon summon && summon.Master.Equals(Master)) return;

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


    private void Die()
    {
        HealthPoints = 0;
        Death(this);
    }

    public override void Death(IThing by)
    {
        base.Death(by);

        Master.Summons.Remove(this);

        if (Master is not ICombatActor actor) return;
        actor.OnDeath -= OnMasterKilled;
        actor.OnTargetChanged -= OnMasterTargetChange;
        actor.OnStoppedAttack -= OnMasterStoppedAttack;

        if (Master is not IPlayer player) return;
        player.OnLoggedOut -= OnMasterLoggedOut;
    }

    private void OnMasterKilled(ICombatActor master, IThing by, ILoot loot)
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