using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Creatures.Monster.Summon;

public class Summon : Monster
{
    public Summon(IMonsterType type, IMapTool mapTool, ICreature master) : base(type, mapTool, null)
    {
        Master = master;
        if (master is not ICombatActor actor) return;

        actor.OnDeath += OnMasterKilled;
        actor.OnTargetChanged += OnMasterTargetChange;
    }

    public ICreature Master { get; }
    public override bool IsSummon => true;

    public override void SetAsEnemy(ICreature creature)
    {
        if (IsDead) return;
        if (Master == creature) return;

        if (creature is Summon summon && summon.Master == Master) return;

        base.SetAsEnemy(creature);
    }

    private void Die()
    {
        HealthPoints = 0;
        Death(this);
    }

    public override void Death(IThing by)
    {
        base.Death(by);

        if (Master is not ICombatActor actor) return;
        actor.OnDeath -= OnMasterKilled;
        actor.OnTargetChanged -= OnMasterTargetChange;
    }

    private void OnMasterKilled(ICombatActor master, IThing by)
    {
        master.OnDeath -= OnMasterKilled;
        master.OnTargetChanged -= OnMasterTargetChange;
        Die();
    }

    private void OnMasterTargetChange(ICombatActor actor, uint oldTargetId, uint newTargetId)
    {
        Targets.Clear();
        SetAsEnemy(actor.CurrentTarget);
    }
}