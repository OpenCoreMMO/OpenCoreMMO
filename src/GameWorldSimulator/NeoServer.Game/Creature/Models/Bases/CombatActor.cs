using NeoServer.Game.Combat;
using NeoServer.Game.Combat.Attacks.Validation;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Creature.Models.Bases;

public abstract class CombatActor : WalkableCreature, ICombatActor
{
    private const byte BLOCK_LIMIT = 2;

    private byte blockCount;
    private bool WasDamagedOnLastAttack = true;

    protected CombatActor(ICreatureType type, IMapTool mapTool, IOutfit outfit = null, uint healthPoints = 0) : base(
        type, mapTool, outfit,
        healthPoints)
    {
    }

    public abstract int DefendUsingShield(int attack);
    public abstract int DefendUsingArmor(int attack);

    public void AddCondition(ICondition condition)
    {
        var result = Conditions.TryAdd(condition.Type, condition);
        condition.Start(this);
        if (result == false) return;

        OnAddedCondition?.Invoke(this, condition);
    }

    public void RemoveCondition(ICondition condition)
    {
        Conditions.Remove(condition.Type);
        OnRemovedCondition?.Invoke(this, condition);
    }

    public void RemoveCondition(ConditionType type)
    {
        if (Conditions.Remove(type, out var condition) is false) return;
        OnRemovedCondition?.Invoke(this, condition);
    }

    public bool HasCondition(ConditionType type, out ICondition condition)
    {
        return Conditions.TryGetValue(type, out condition);
    }

    public bool HasCondition(ConditionType type)
    {
        return Conditions.ContainsKey(type);
    }

    public void ResetHealthPoints()
    {
        Heal((ushort)MaxHealthPoints, this);
    }

    public virtual void GainExperience(long exp)
    {
        OnGainedExperience?.Invoke(this, exp);
    }

    public virtual void LoseExperience(long exp)
    {
        OnLoseExperience?.Invoke(this, exp);
    }

    public override void OnMoved(IDynamicTile fromTile, IDynamicTile toTile, ICylinderSpectator[] spectators)
    {
        base.OnMoved(fromTile, toTile, spectators);
        if (CurrentTarget is null) return;

        if (AttackValidationStatic.CanAttack(this, CurrentTarget as ICombatActor).Failed) StopAttack();
    }

    public CombatDamage ReduceDamage(CombatDamage attack)
    {
        int damage = attack.Damage;

        if (CanBlock(attack))
        {
            damage = DefendUsingShield(attack.Damage);

            if (damage <= 0)
            {
                damage = 0;

                UpdateBlockCounter(BlockType.Shield);
                attack.SetNewDamage((ushort)damage);
                return attack;
            }
        }

        if (!attack.IsElementalDamage) damage = DefendUsingArmor(attack.Damage);

        if (damage <= 0)
        {
            damage = 0;
            OnBlockedAttack?.Invoke(this, BlockType.Armor);
        }

        attack.SetNewDamage((ushort)damage);

        attack = OnImmunityDefense(attack);

        if (attack.Damage <= 0) OnBlockedAttack?.Invoke(this, BlockType.Armor);

        return attack;
    }

    public void StopAttack()
    {
        if (!Attacking) return;

        StopFollowing();
        CurrentTarget = null;
        OnStoppedAttack?.Invoke(this);
    }

    public Result Attack(ICreature creature, IUsableAttackOnCreature item)
    {
        // if (creature is not ICombatActor enemy || enemy.IsDead || IsDead || !CanSee(creature.Location) ||
        //     creature.Equals(this) || creature.IsInvisible)
        // {
        //     StopAttack();
        //     return Result.NotApplicable;
        // }
        //
        // if (creature.Tile.ProtectionZone || Tile.ProtectionZone)
        // {
        //     OperationFailService.Send(CreatureId, TextConstants.NOT_PERMITTED_IN_PROTECTION_ZONE);
        //     return Result.Fail(InvalidOperation.NotPermittedInProtectionZone);
        // }
        //
        // if (item.NeedTarget && MapTool.SightClearChecker?.Invoke(Location, enemy.Location, true) == false)
        // {
        //     OperationFailService.Send(CreatureId, "You cannot throw there.");
        //     return Result.Fail(InvalidOperation.CannotThrowThere);
        // }
        //
        // var useOperationResult = item.Use(this, creature, out var combat);
        // if (useOperationResult.Failed) return useOperationResult;
        //
        // OnAttackEnemy?.Invoke(this, enemy, new[] { combat });

        return Result.Success;
    }

    public virtual Result Attack(ICombatActor enemy)
    {
        // var canAttackResult = AttackValidation.CanAttack(this, enemy);
        // if (canAttackResult.Failed)
        // {
        //     StopAttack();
        //     return canAttackResult;
        // }
        //
        // if (!Cooldowns.Expired(CooldownType.Combat)) return Result.Fail(InvalidOperation.CannotAttackThatFast);
        //
        // SetAttackTarget(enemy);
        //
        // if (MapTool.SightClearChecker?.Invoke(Location, enemy.Location, true) == false)
        //     return Result.Fail(InvalidOperation.CreatureIsNotReachable);
        //
        // var attackResult = OnAttack(enemy, out var combat);
        // if (attackResult.Failed) return attackResult;
        //
        // OnAttackEnemy?.Invoke(this, enemy, combat);
        //
        // Cooldowns.Start(CooldownType.Combat, (int)AttackSpeed);

        return Result.Success;
    }

    public override void OnAppear(Location location, ICylinderSpectator[] spectators)
    {
        foreach (var cylinderSpectator in spectators)
        {
            var spectator = cylinderSpectator.Spectator;

            if (spectator is not ICombatActor spectatorEnemy) continue;
            if (spectator.GetType() == GetType()) continue;

            spectatorEnemy.OnEnemyAppears(this);

            if (!spectatorEnemy.IsHostileTo(this)) continue;
            if (!spectatorEnemy.Location.SameFloorAs(Location)) continue;

            SetAsEnemy(spectatorEnemy);
        }
    }

    public abstract bool IsHostileTo(ICombatActor enemy);

    public void OnEnemyAppears(ICombatActor enemy)
    {
        if (!enemy.IsHostileTo(this)) return;
        SetAsEnemy(enemy);
    }

    public virtual Result SetAttackTarget(ICreature target)
    {
        if (target is not ICombatActor enemy) return Result.NotPossible;
        if (target?.CreatureId == AutoAttackTargetId) return Result.NotPossible;

        var canAttackResult = AttackValidationStatic.CanAttack(this, enemy);

        if (canAttackResult.Failed)
        {
            InvokeAttackCanceled();
            return canAttackResult;
        }

        var oldAttackTarget = AutoAttackTargetId;
        CurrentTarget = target;

        if (target?.CreatureId == 0)
        {
            StopAttack();
            StopFollowing();
        }

        OnTargetChanged?.Invoke(this, oldAttackTarget, target?.CreatureId ?? default);
        return Result.Success;
    }

    public void Heal(ushort increasing, ICreature healedBy)
    {
        if (increasing <= 0) return;

        if (HealthPoints == MaxHealthPoints) return;

        HealthPoints = HealthPoints + increasing >= MaxHealthPoints ? MaxHealthPoints : HealthPoints + increasing;
        OnHeal?.Invoke(this, healedBy, increasing);
    }

    public virtual void TurnInvisible()
    {
        IsInvisible = true;
        OnChangedVisibility?.Invoke(this);
    }

    // public override Direction GetNextStep()
    // {
    //     // if (IsDead) return Direction.None;
    //     // return base.GetNextStep();
    //     return Direction.None;
    // }

    public virtual void TurnVisible()
    {
        IsInvisible = false;
        OnChangedVisibility?.Invoke(this);
    }

    public void StartSpellCooldown(ISpell spell)
    {
        Cooldowns.Start(spell.Name, (int)spell.Cooldown);
    }

    public bool SpellCooldownHasExpired(ISpell spell)
    {
        return Cooldowns.Expired(spell.Name);
    }

    public bool CooldownHasExpired(CooldownType type)
    {
        return Cooldowns.Expired(type);
    }

    public virtual bool ReceiveAttackFrom(IThing enemy, CombatDamage damage) =>
        ReceiveAttackFrom(enemy, new CombatDamageList([damage]));

    public virtual bool ReceiveAttackFrom(IThing enemy, CombatDamageList damageList)
    {
        // if (enemy?.Equals(this) ?? false) return false;
        // if (!CanBeAttacked) return false;
        // if (IsDead) return false;
        //
        // OnAttacked?.Invoke(enemy, this, ref damageList);
        //
        // if (enemy is ICreature c) SetAsEnemy(c);
        //
        // foreach (var damage in damageList.Damages)
        // {
        //     damage = ReduceDamage(damage);
        //     if (damage.Damage <= 0)
        //     {
        //         WasDamagedOnLastAttack = false;
        //         continue;
        //     }
        //
        //     if (damage.Damage > HealthPoints) damage.SetNewDamage((ushort)HealthPoints);
        // }
        //
        // ProcessDamage(enemy, this, damageList);
        //
        // WasDamagedOnLastAttack = true;
        return true;
    }

    public void PropagateAttack(AffectedLocation[] area, CombatDamage damage)
    {
        if (IsDead) return;
        if (damage.Damage <= 0) return;

        OnPropagateAttack?.Invoke(this, damage, area);
    }

    public void PropagateAttack(AffectedLocation area, CombatDamage damage)
    {
        PropagateAttack(new[] { area }, damage);
    }

    public abstract void SetAsEnemy(ICreature actor);
    public Result Attack(ICombatActor enemy, ICombatAttack attack, CombatAttackValue value)
    {
        // if (enemy?.IsInvisible ?? false) return Result.Fail(InvalidOperation.AttackTargetIsInvisible);
        //
        // if (Guard.AnyNull(attack)) return Result.Fail(InvalidOperation.Impossible);
        //
        // if (enemy is not null && CanAttack(enemy).Failed) return Result.Fail(InvalidOperation.Impossible);
        //
        // if (enemy is not null && MapTool.SightClearChecker?.Invoke(Location, enemy.Location, true) == false)
        //     return Result.Fail(InvalidOperation.CreatureIsNotReachable);
        //
        // var result = attack.TryAttack(this, enemy, value, out var combatAttackType);
        // if (result is false) return Result.Fail(InvalidOperation.Impossible);
        //
        // OnAttackEnemy?.Invoke(this, enemy, new[] { combatAttackType });
        return Result.Success;
    }

    public virtual void PreAttack(PreAttackValues preAttackValues)
    {
        OnAttackEnemy?.Invoke(preAttackValues);
    }
    public virtual void PostAttack(AttackInput attackInput)
    {
        var cooldownType = attackInput.Parameters.CooldownType;
        if (cooldownType is not CooldownType.None && Cooldowns.Expired(cooldownType))
        {
            Cooldowns.Start(cooldownType, (int)AttackSpeed);
        }
    }

    public abstract bool HasImmunity(Immunity immunity);

    public virtual bool CanBlock(CombatDamage damage)
    {
        if (damage.IsElementalDamage) return false;
        var hasCoolDownExpired = Cooldowns.Expired(CooldownType.Block);

        if (!hasCoolDownExpired && blockCount >= BLOCK_LIMIT) return false;
        return true;
    }

    public void UpdateBlockCounter(BlockType blockType)
    {
        if (blockType is BlockType.Shield && Cooldowns.Expired(CooldownType.Block))
        {
            Cooldowns.Start(CooldownType.Block, 2000);
            blockCount = 0;
        }

        OnBlockedAttack?.Invoke(this, blockType);

        blockCount++;
    }

    public Result Attack(ITile tile, IUsableAttackOnTile item)
    {
        // if (tile.ProtectionZone || Tile.ProtectionZone)
        // {
        //     OperationFailService.Send(CreatureId, TextConstants.NOT_PERMITTED_IN_PROTECTION_ZONE);
        //     return Result.Fail(InvalidOperation.CannotAttackWhileInProtectionZone);
        // }
        //
        // if (!CanSee(tile.Location)) return Result.Fail(InvalidOperation.CannotThrowThere);
        //
        // if (MapTool.SightClearChecker?.Invoke(Location, tile.Location, true) == false)
        // {
        //     OperationFailService.Send(CreatureId, TextConstants.YOU_CANNOT_THROW_THERE);
        //     return Result.Fail(InvalidOperation.CannotThrowThere);
        //     ;
        // }
        //
        // var useOperationResult = item.Use(this, tile, out var combat);
        // if (useOperationResult.Failed) return useOperationResult;
        //
        // var creature = tile is IDynamicTile t ? tile.TopCreatureOnStack : null;
        // OnAttackEnemy?.Invoke(this, creature, new[] { combat });

        return Result.Success;
    }

    public virtual Result CanAttack(ITile target)
    {
        if (Guard.IsNull(target) || Equals(target)) return Result.NotPossible;
        
        if (!CanSee(target.Location)) return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        if (!Location.SameFloorAs(target.Location))
            return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        if (Tile?.ProtectionZone ?? false)
            return Result.Fail(InvalidOperation.CannotAttackWhileInProtectionZone);

        if (target.ProtectionZone)
            return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);
        
        return Result.Success;
    }
    public virtual Result CanAttack(ICombatActor victim)
    {
        if (Guard.IsNull(victim) || Equals(victim)) return Result.NotPossible;

        if (victim.IsDead || IsDead) return Result.Fail(InvalidOperation.CreatureIsDead);

        if (!CanSee(victim.Location)) return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        if (!Location.SameFloorAs(victim.Location))
            return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        if (Tile?.ProtectionZone ?? false)
            return Result.Fail(InvalidOperation.CannotAttackWhileInProtectionZone);

        if (victim.Tile?.ProtectionZone ?? false)
            return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);

       // if (!Cooldowns.Expired(CooldownType.Combat)) return Result.Fail(InvalidOperation.CannotAttackThatFast);

        return Result.Success;
    }

    protected void ReduceHealth(CombatDamage damage)
    {
        HealthPoints = damage.Damage > HealthPoints ? 0 : HealthPoints - damage.Damage;
    }

    public abstract ILoot DropLoot();

    public virtual void Die(IThing by)
    {
        StopAttack();
        StopFollowing();
        StopWalking();
        Conditions.Clear();
        var loot = DropLoot();
        OnKilled?.Invoke(this, by, loot);
    }

    public abstract void ProcessDamage(IThing enemy, CombatDamage damage);

    public void ProcessDamage(IThing enemy, ICombatActor actor, CombatDamageList damageList)
    {
        OnAttacked?.Invoke(enemy, this, ref damageList);
        
        foreach (var damage in damageList.Damages)
        {
            if (damage.Damage <= 0) continue;
            
            ProcessDamage(enemy, damage);
            if (IsDead) break;
        }

        OnInjured?.Invoke(enemy, this, damageList);
        if (IsDead) Die(enemy);
    }

    public abstract CombatDamage OnImmunityDefense(CombatDamage damage);

    protected void InvokeAttackCanceled()
    {
        OnAttackCanceled?.Invoke(this);
    }

    #region Events

    public event Heal OnHeal;
    public event StopAttack OnStoppedAttack;
    public event StopAttack OnAttackCanceled;
    public event BlockAttack OnBlockedAttack;
    public event Attack OnAttackEnemy;
    public event Damage OnInjured;
    public event Die OnKilled;
    public event AttackTargetChange OnTargetChanged;
    public event ChangeVisibility OnChangedVisibility;
    public event PropagateAttack OnPropagateAttack;
    public event GainExperience OnGainedExperience;
    public event LoseExperience OnLoseExperience;
    public event AddCondition OnAddedCondition;
    public event RemoveCondition OnRemovedCondition;
    public event Attacked OnAttacked;

    #endregion

    #region Properties

    public bool IsDead => HealthPoints <= 0;
    public virtual decimal AttackSpeed => 2000M;
    public decimal BaseDefenseSpeed { get; }
    public bool InFight => HasCondition(ConditionType.InFight);
    public abstract ushort ArmorRating { get; }
    public uint AutoAttackTargetId => CurrentTarget?.CreatureId ?? default;
    public ICreature CurrentTarget { get; private set; }
    public bool Attacking => AutoAttackTargetId > 0;
    public abstract ushort MinimumAttackPower { get; }
    public abstract ushort MaximumAttackPower { get; }
    public abstract ushort MaximumElementalAttackPower { get; }

    public abstract bool UsingDistanceWeapon { get; }
    public virtual bool CanBeAttacked => true; //todo: set as a flag

    public IDictionary<ConditionType, ICondition> Conditions { get; set; } =
        new Dictionary<ConditionType, ICondition>();

    #endregion
}