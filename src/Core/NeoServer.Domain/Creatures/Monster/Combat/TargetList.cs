using System.Collections;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Monster.Combat;

public class TargetList : IEnumerable<CombatTarget>
{
    private readonly IMonster _monster;
    private IDictionary<uint, CombatTarget> _targets = new Dictionary<uint, CombatTarget>();

    public TargetList(IMonster monster)
    {
        _monster = monster;
    }

    public CombatTarget NearestTarget { private get; set; }
    public CombatTarget NearestSightClearTarget { private get; set; }

    public bool CanAttackAnyTarget
    {
        get
        {
            var target = NearestTarget ?? NearestSightClearTarget;
            return target is not null &&
                   target.CanReachCreature;
        }
    }

    public CombatTarget PossibleTargetToAttack
    {
        get
        {
            if (NearestTarget is not null) return NearestTarget;
            if (!_monster.Metadata.HasDistanceAttack) return NearestTarget;

            if (NearestSightClearTarget is null) return null;

            if (!NearestSightClearTarget.IsInRange(_monster)) return null;

            return NearestSightClearTarget;
        }
    }

    public bool IsCurrentTargetUnreachable =>
        TryGetTarget(_monster.CurrentTarget?.CreatureId ?? 0, out var target) && !target.CanReachCreature &&
        target.Creature.Tile.ProtectionZone &&
        !target.HasSightClear;

    public void AddTarget(ICombatActor creature)
    {
        _targets ??= new Dictionary<uint, CombatTarget>(150);

        if (!_targets.TryAdd(creature.CreatureId, new CombatTarget(creature))) return;
        AttachToTargetEvents(creature);
    }

    public void RemoveTarget(ICreature creature)
    {
        if (creature is ICombatActor actor) DettachFromTargetEvents(actor);

        _targets?.Remove(creature.CreatureId);

        if (_monster.AutoAttackTargetId == creature.CreatureId) _monster.StopAttack();
    }

    public void Clear()
    {
        if (_targets is null) return;
        foreach (var target in _targets) RemoveTarget(target.Value.Creature);
    }

    public bool Any()
    {
        return _targets?.Any() ?? false;
    }

    public bool TryGetTarget(uint id, out CombatTarget target)
    {
        target = null;
        return _targets?.TryGetValue(id, out target) ?? false;
    }

    #region Target Event Handlers

    private void AttachToTargetEvents(ICombatActor creature)
    {
        creature.OnDeath += OnTargetDie;
        creature.OnChangedVisibility += OnTargetDisappeared;
        creature.OnCreatureMoved += OnTargetMoved;
        if (creature is IPlayer player) player.OnLoggedOut += OnTargetRemoved;
    }

    private void DettachFromTargetEvents(ICombatActor creature)
    {
        creature.OnDeath -= OnTargetDie;
        creature.OnChangedVisibility -= OnTargetDisappeared;
        creature.OnCreatureMoved -= OnTargetMoved;
        if (creature is IPlayer player) player.OnLoggedOut -= OnTargetRemoved;
    }

    private void OnTargetDie(ICreature creature, IThing by)
    {
        RemoveTarget(creature);
    }

    private void OnTargetDisappeared(ICreature creature)
    {
        if (_monster.CanSee(creature)) return;
        RemoveTarget(creature);
    }

    private void HandleTargetMoved(IWalkableCreature creature, bool lastStep = false)
    {
        if (!_monster.CanSee(creature.Location)) RemoveTarget(creature);
    }

    private void OnTargetMoved(IWalkableCreature creature, Location fromLocation, Location toLocation,
        ICylinderSpectator[] spectators)
    {
        HandleTargetMoved(creature);
    }

    private void OnTargetRemoved(ICreature creature)
    {
        RemoveTarget(creature);
    }

    #endregion

    #region IEnumerable Implementations

    public IEnumerator GetEnumerator()
    {
        return _targets.Values.GetEnumerator();
    }

    IEnumerator<CombatTarget> IEnumerable<CombatTarget>.GetEnumerator()
    {
        return _targets.Values.GetEnumerator();
    }

    #endregion
}