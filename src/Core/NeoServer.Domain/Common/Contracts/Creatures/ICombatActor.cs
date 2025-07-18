using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void AttackTargetChange(ICombatActor actor, uint oldTargetId, uint newTargetId);

public delegate void ManaChange(ICombatActor actor, ICreature attacker, CombatDamage damage);

public delegate void Heal(ICombatActor healedCreature, ICreature healingCreature, ushort amount);

public delegate void StopAttack(ICombatActor actor);

public delegate void BlockAttack(ICombatActor creature, BlockType block);

public delegate void Attack(ICombatActor creature, ICreature victim, CombatAttackResult[] combatAttacks);

public delegate void UseSpell(ICreature creature, ISpell spell);

public delegate void ChangeVisibility(ICombatActor actor);

public delegate void PropagateAttack(ICombatActor actor, CombatDamage damage, AffectedLocation[] area);

public delegate void DropLoot(ICombatActor actor, Loot loot);

public interface ICombatActor : IWalkableCreature
{
    ushort ArmorRating { get; }
    bool Attacking { get; }
    uint AutoAttackTargetId { get; }
    decimal AttackSpeed { get; }
    decimal BaseDefenseSpeed { get; }
    bool IsDead { get; }
    ushort MinimumAttackPower { get; }
    ushort MaximumAttackPower { get; }
    ushort MaximumElementalAttackPower { get; }
    bool UsingDistanceWeapon { get; }
    uint AttackEvent { get; set; }
    bool CanBeAttacked { get; }
    IDictionary<ConditionType, ICondition> Conditions { get; set; }
    ICreature CurrentTarget { get; }
    DamageRecordList ReceivedDamages { get; }

    event Attack OnAttackEnemy;
    event BlockAttack OnBlockedAttack;
    event Heal OnHeal;
    event BeforeDeath OnBeforeDeath;
    event Death OnDeath;
    event StopAttack OnStoppedAttack;
    event AttackTargetChange OnTargetChanged;
    event ChangeVisibility OnChangedVisibility;
    event PropagateAttack OnPropagateAttack;
    event GainExperience OnGainedExperience;

    int DefendUsingArmor(int attack);
    Result Attack(ICombatActor enemy, ICombatAttack attack, CombatAttackValue value);
    void Heal(ushort increasing, ICreature healedBy);
    CombatDamage ReduceDamage(CombatDamage damage);
    Result SetAttackTarget(ICreature target);
    int DefendUsingShield(int attack);
    void StopAttack(bool force = false);
    void ResetHealthPoints();
    void TurnInvisible();
    void TurnVisible();
    void StartCooldown(IHasCooldown cooldown);
    bool CooldownHasExpired(IHasCooldown cooldown);
    bool CooldownHasExpired(CooldownType type);

    /// <summary>
    ///     Creature receive attack damage from enemy
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="damages"></param>
    /// <returns>Returns true when damage was bigger than 0</returns>
    DamageResult TakeDamage(IThing enemy, CombatDamageList damages);

    DamageResult TakeDamage(IThing enemy, CombatDamage damages);
    Result Attack(ICombatActor creature);
    void PropagateAttack(AffectedLocation[] area, CombatDamage damage);
    bool Attack(ICreature creature, IUsableAttackOnCreature item);

    /// <summary>
    ///     Set creature as enemy. If monster can't see creature it will be forgotten
    /// </summary>
    void SetAsEnemy(ICreature actor);

    void GainExperience(long exp);
    void LoseExperience(long exp);
    void AddCondition(ICondition condition);
    void RemoveCondition(ICondition condition);
    void DisableCondition(ConditionType type);
    void EnableCondition(ConditionType type);
    void RemoveCondition(ConditionType type);
    bool HasCondition(ConditionType type, out ICondition condition);
    bool HasCondition(ConditionType type);
    ICondition GetCondition(ConditionType type);
    void PropagateAttack(AffectedLocation area, CombatDamage damage);
    void OnEnemyAppears(ICombatActor enemy);
    bool IsHostileTo(ICombatActor enemy);
    Result OnAttack(ICombatActor enemy, out CombatAttackResult[] combatAttacks);

    event StopAttack OnAttackCanceled;
    void DisableShieldDefense();
    void EnableShieldDefense();
    void IncreaseDamageReceived(byte percentage);
    void DecreaseDamageReceived(byte percentage);
    void Kill(ICombatActor enemy, bool lastHit = false, bool justified = true);
    void RaiseDroppedLootEvent(ICombatActor actor, Loot loot);
    event DropLoot OnDroppedLoot;
    void PreAttack(CombatContext combatContext);
    Result CanAttack(CombatParameter combatParameter);
    void StartCooldown(Guid cooldownId, uint duration);
}