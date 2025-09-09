using NeoServer.Domain.Combat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Models.Bases.Events;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Creatures.Models.Bases;

public abstract class CombatActor : WalkableCreature, ICombatActor
{
    private const byte BLOCK_LIMIT = 2;

    private byte _blockCount;

    protected CombatActor(ICreatureType type, IMapTool mapTool, IOutfit outfit = null, uint healthPoints = 0) : base(
        type, mapTool, outfit,
        healthPoints)
    {
    }

    public bool IsShieldDefenseEnabled { get; private set; } = true;
    public byte DamageReceivedPercentage { get; private set; }

    public DamageRecordList ReceivedDamages { get; } = new();

    public abstract int DefendUsingShield(int attack);
    public abstract int DefendUsingArmor(int attack);

    public virtual void AddCondition(ICondition condition)
    {
        switch (condition.Type)
        {
            case ConditionType.Haste:
                Conditions.TryGetValue(ConditionType.Paralyze, out var paralyzeCondition);
                paralyzeCondition?.End();
                break;
            case ConditionType.Paralyze:
                Conditions.TryGetValue(ConditionType.Haste, out var hasteCondition);
                hasteCondition?.End();
                break;
        }

        var result = Conditions.TryAdd(condition.Type, condition);
        condition.Start(this);
        if (result == false) return;

        EventAggregator.Invoke(new CreatureConditionAddedEvent(this, condition));
    }

    public void RemoveCondition(ICondition condition)
    {
        Conditions.Remove(condition.Type);
        EventAggregator.Invoke(new CreatureConditionRemovedEvent(this, condition));
    }

    public void DisableCondition(ConditionType type)
    {
        if (!Conditions.TryGetValue(type, out var condition)) return;

        condition.Disable();
        EventAggregator.Invoke(new CreatureConditionRemovedEvent(this, condition));
    }

    public void EnableCondition(ConditionType type)
    {
        if (!Conditions.TryGetValue(type, out var condition)) return;

        condition.Enable();
        EventAggregator.Invoke(new CreatureConditionAddedEvent(this, condition));
    }

    public void RemoveCondition(ConditionType type)
    {
        if (Conditions.Remove(type, out var condition) is false) return;
        EventAggregator.Invoke(new CreatureConditionRemovedEvent(this, condition));
    }

    public bool HasCondition(ConditionType type, out ICondition condition)
    {
        return Conditions.TryGetValue(type, out condition) && !condition.IsDisabled;
    }

    public bool HasCondition(ConditionType type)
    {
        return Conditions.TryGetValue(type, out var condition) && !condition.IsDisabled;
    }

    public ICondition GetCondition(ConditionType type)
    {
        Conditions.TryGetValue(type, out var condition);
        return condition;
    }

    public void ResetHealthPoints()
    {
        Heal((ushort)MaxHealthPoints, this);
    }

    public virtual void GainExperience(long experience)
    {
        OnGainedExperience?.Invoke(this, experience);
    }

    public virtual void LoseExperience(long exp)
    {
        OnLoseExperience?.Invoke(this, exp);
    }

    public virtual CombatDamage ReduceDamage(CombatDamage attack)
    {
        int damage = attack.Damage;

        damage += damage * DamageReceivedPercentage / 100;

        if (CanBlock(attack.Type))
        {
            damage = DefendUsingShield(damage);

            if (damage <= 0)
            {
                damage = 0;

                Block();
                OnBlockedAttack?.Invoke(this, BlockType.Shield);
                attack.SetNewDamage((ushort)damage);
                return attack;
            }
        }

        if (!attack.IsElementalDamage)
        {
            damage = DefendUsingArmor(damage);

            if (damage <= 0)
            {
                damage = 0;
                OnBlockedAttack?.Invoke(this, BlockType.Armor);
            }
        }

        attack.SetNewDamage((ushort)damage);

        if (attack.Damage <= 0) return attack;

        attack = OnImmunityDefense(attack);

        if (attack.Damage <= 0) OnBlockedAttack?.Invoke(this, BlockType.Armor);

        return attack;
    }

    public void StopAttack(bool force = false)
    {
        if (force is false && !Attacking) return;

        StopFollowing();
        CurrentTarget = null;
        OnStoppedAttack?.Invoke(this);
    }

    public virtual Result CanAttack(CombatParameter combatParameter)
    {
        if (IsDead) return Result.Fail(InvalidOperation.CreatureIsDead);

        if (combatParameter.CooldownType is not CooldownType.None && !Cooldowns.Expired(combatParameter.CooldownType))
            return Result.Fail(InvalidOperation.CannotAttackThatFast);

        if (this is IPlayer player && player.Group.FlagIsEnabled(PlayerFlag.IgnoreProtectionZone))
            return Result.Success;

        if (Tile?.ProtectionZone ?? false)
            return Result.Fail(InvalidOperation.CannotAttackWhileInProtectionZone);

        return Result.Success;
    }

    public DamageResult TakeDamage(IThing enemy, CombatDamage damages)
    {
        return TakeDamage(enemy, new CombatDamageList(damages));
    }

    public override void Appear(Location location, ICylinderSpectator[] spectators)
    {
        base.Appear(location, spectators);
        foreach (var cylinderSpectator in spectators)
        {
            var spectator = cylinderSpectator.Spectator;

            if (spectator is IPlayer player && player.Group.FlagIsEnabled(PlayerFlag.IgnoredByMonsters)) continue;
            if (spectator is not ICombatActor spectatorEnemy) continue;
            if (spectator.GetType() == GetType()) continue;

            spectatorEnemy.OnEnemyAppears(this);

            if (!spectatorEnemy.IsHostileTo(this)) continue;
            if (!spectatorEnemy.Location.SameFloorAs(Location)) continue;


            SetAsEnemy(spectatorEnemy);
        }
    }

    public override void Disappear(Location location, ICylinderSpectator[] spectators)
    {
        base.Disappear(location, spectators);
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

        var oldAttackTarget = AutoAttackTargetId;
        CurrentTarget = target;

        if (target?.CreatureId == 0)
        {
            StopAttack();
            StopFollowing();
        }

        OnTargetChanged?.Invoke(this, oldAttackTarget, (uint)target?.CreatureId);
        return Result.Success;
    }

    public void Heal(ushort increasing, ICreature healedBy)
    {
        if (increasing <= 0) return;

        if (HealthPoints == MaxHealthPoints) return;

        var oldHealthPoints = HealthPoints;

        HealthPoints = HealthPoints + increasing >= MaxHealthPoints ? MaxHealthPoints : HealthPoints + increasing;

        OnHeal?.Invoke(this, healedBy, increasing);
        EventAggregator.Invoke(new CreatureHealthChangedEvent(this, oldHealthPoints, HealthPoints));
    }

    public virtual void TurnInvisible()
    {
        IsInvisible = true;
        OnChangedVisibility?.Invoke(this);
    }

    public override Direction GetNextStep()
    {
        if (IsDead) return Direction.None;
        return base.GetNextStep();
    }

    public virtual void TurnVisible()
    {
        IsInvisible = false;
        OnChangedVisibility?.Invoke(this);
    }

    public void StartCooldown(Guid cooldownId, uint duration)
    {
        Cooldowns.Start(cooldownId, duration);
    }

    public void StartCooldown(IHasCooldown cooldown)
    {
        Cooldowns.Start(cooldown);
    }

    public bool CooldownHasExpired(IHasCooldown cooldown)
    {
        return Cooldowns.Expired(cooldown);
    }

    public bool CooldownHasExpired(CooldownType type)
    {
        return Cooldowns.Expired(type);
    }

    public virtual DamageResult TakeDamage(IThing enemy, CombatDamageList damages)
    {
        if (enemy?.Equals(this) ?? false) return new DamageResult(damages, false);
        if (!CanBeAttacked) return new DamageResult(damages, false);
        if (IsDead) return new DamageResult(damages, false);

        if (enemy is ICreature c) SetAsEnemy(c);

        var wasDamaged = false;

        foreach (var damage in damages)
        {
            if (damage.Type is DamageType.None) continue;

            ReduceDamage(damage);

            if (damage.Damage <= 0) continue;

            if (damage.Damage > HealthPoints) damage.SetNewDamage((ushort)HealthPoints);

            wasDamaged = true;
        }

        OnDamage(enemy, this, damages);

        return new DamageResult(damages, wasDamaged);
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

    public abstract Result OnAttack(ICombatActor enemy, out CombatAttackResult[] combatAttacks);
    
    public void DisableShieldDefense()
    {
        IsShieldDefenseEnabled = false;
    }

    public void EnableShieldDefense()
    {
        IsShieldDefenseEnabled = true;
    }

    public void IncreaseDamageReceived(byte percentage)
    {
        DamageReceivedPercentage += percentage;
    }

    public void DecreaseDamageReceived(byte percentage)
    {
        DamageReceivedPercentage -= percentage;
    }

    public void RaiseDroppedLootEvent(ICombatActor actor, Loot loot)
    {
        OnDroppedLoot?.Invoke(actor, loot);
    }

    public virtual void Kill(ICombatActor enemy, bool lastHit = false, bool unjustified = false)
    {
        EventAggregator.Invoke(new CreatureKillEvent(this, enemy, lastHit, unjustified));
    }

    public virtual void PreAttack(CombatContext combatContext)
    {
        Cooldowns.Start(combatContext.CombatParameters.CooldownType, combatContext.CombatParameters.CooldownDuration);
    }

    public virtual CalculatedAttackDamage CalculateAttackDamage()
    {
        return new CalculatedAttackDamage();
    }

    public abstract bool HasImmunity(Immunity immunity);

    public virtual bool CanBlock(DamageType damage)
    {
        if (damage != DamageType.Melee) return false;
        if (!IsShieldDefenseEnabled) return false;
        var hasCoolDownExpired = Cooldowns.Expired(CooldownType.Block);

        if (!hasCoolDownExpired && _blockCount >= BLOCK_LIMIT) return false;
        return true;
    }

    private void Block()
    {
        if (Cooldowns.Expired(CooldownType.Block))
        {
            Cooldowns.Start(CooldownType.Block, 2000);
            _blockCount = 0;
        }

        _blockCount++;
    }
    protected void ReduceHealth(CombatDamage damage)
    {
        ReduceHealth(damage.Damage);
    }

    protected void ReduceHealth(ushort damage)
    {
        HealthPoints = damage > HealthPoints ? 0 : HealthPoints - damage;
    }

    public virtual void Death(IThing by)
    {
        if (by is ICombatActor combatActor)
            //todo: implements real damage
            OnBeforeDeath?.Invoke(this, combatActor, 0);

        OnDeath?.Invoke(this, by);
        EventAggregator.Invoke(new CreatureDeathEvent(this, by));
        
        Dismiss();
    }

    public virtual void Dismiss()
    {
        StopAttack();
        StopFollowing();
        StopWalking();
        Conditions.Clear();
        ReceivedDamages.Clear();
    }

    public abstract void OnDamage(IThing enemy, CombatDamageList damages);

    private void OnDamage(IThing enemy, ICombatActor actor, CombatDamageList damages)
    {
        OnDamage(enemy, damages);

        ReceivedDamages.AddOrUpdateDamage(enemy, damages.TotalDamage, damages.Unjustified);

        EventAggregator.Invoke(new CreatureInjuredEvent(enemy, this, damages));

        if (IsDead) Death(enemy);
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
    public event BeforeDeath OnBeforeDeath;
    public event Death OnDeath;
    public event AttackTargetChange OnTargetChanged;
    public event ChangeVisibility OnChangedVisibility;
    public event PropagateAttack OnPropagateAttack;
    public event GainExperience OnGainedExperience;
    public event LoseExperience OnLoseExperience;
    public event DropLoot OnDroppedLoot;

    #endregion

    #region Properties

    public bool IsDead => HealthPoints <= 0;
    public virtual decimal AttackSpeed => 2000M;
    public decimal BaseDefenseSpeed { get; }
    public abstract ushort ArmorRating { get; }
    public uint AutoAttackTargetId => CurrentTarget?.CreatureId ?? default;
    public ICreature CurrentTarget { get; private set; }
    public bool Attacking => AutoAttackTargetId > 0;
    public abstract ushort MinimumAttackPower { get; }
    public abstract bool UsingDistanceWeapon { get; }
    public uint AttackEvent { get; set; }
    public virtual bool CanBeAttacked => true; //todo: set as a flag

    public IDictionary<ConditionType, ICondition> Conditions { get; set; } =
        new Dictionary<ConditionType, ICondition>();

    public abstract ushort MaximumAttackPower { get; }
    public abstract ushort MaximumElementalAttackPower { get; }

    #endregion
}