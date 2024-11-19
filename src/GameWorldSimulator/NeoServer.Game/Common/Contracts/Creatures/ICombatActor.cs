using NeoServer.Game.Combat;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void AttackTargetChange(ICombatActor actor, uint oldTargetId, uint newTargetId);

public delegate void Damage(IThing enemy, ICombatActor victim, CombatDamageList damageList);

public delegate void Attacked(IThing enemy, ICombatActor victim, ref CombatDamageList damage);

public delegate void Heal(ICombatActor healedCreature, ICreature healingCreature, ushort amount);

public delegate void StopAttack(ICombatActor actor);

public delegate void BlockAttack(ICombatActor creature, BlockType block);

public delegate void Attack(PreAttackValues preAttackValues);

public delegate void UseSpell(ICreature creature, ISpell spell);

public delegate void ChangeVisibility(ICombatActor actor);

public delegate void PropagateAttack(ICombatActor actor, CombatDamage damage, AffectedLocation[] area);

public delegate void DropLoot(ICombatActor actor, ILoot loot);

public interface ICombatActor : IWalkableCreature
{
    public ushort ArmorRating { get; }
    bool Attacking { get; }
    uint AutoAttackTargetId { get; }
    decimal AttackSpeed { get; }
    decimal BaseDefenseSpeed { get; }

    bool InFight { get; }
    bool IsDead { get; }
    ushort MinimumAttackPower { get; }
    ushort MaximumAttackPower { get; }
    bool UsingDistanceWeapon { get; }
    bool CanBeAttacked { get; }
    IDictionary<ConditionType, ICondition> Conditions { get; set; }
    ICreature CurrentTarget { get; }
    bool HasTarget => CurrentTarget != null;
    ushort MaximumElementalAttackPower { get; }
    event Attack OnAttackEnemy;
    event BlockAttack OnBlockedAttack;
    event Damage OnInjured;
    event Heal OnHeal;
    event Die OnKilled;
    event StopAttack OnStoppedAttack;
    event AttackTargetChange OnTargetChanged;
    event ChangeVisibility OnChangedVisibility;
    event PropagateAttack OnPropagateAttack;
    event GainExperience OnGainedExperience;
    event RemoveCondition OnRemovedCondition;
    event AddCondition OnAddedCondition;
    event Attacked OnAttacked;
    int DefendUsingArmor(int attack);
    Result Attack(ICombatActor enemy, ICombatAttack attack, CombatAttackValue value);
    void Heal(ushort increasing, ICreature healedBy);
    CombatDamage ReduceDamage(CombatDamage damage);
    Result SetAttackTarget(ICreature target);
    int DefendUsingShield(int attack);
    void StopAttack();
    void ResetHealthPoints();
    void TurnInvisible();
    void TurnVisible();
    void StartSpellCooldown(ISpell spell);
    bool SpellCooldownHasExpired(ISpell spell);
    bool CooldownHasExpired(CooldownType type);

    /// <summary>
    ///     Creature receive attack damage from enemy
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="damageList"></param>
    /// <returns>Returns true when damage was bigger than 0</returns>
    bool ReceiveAttackFrom(IThing enemy, CombatDamageList damageList);

    Result Attack(ICombatActor creature);
    void PropagateAttack(AffectedLocation[] area, CombatDamage damage);
    Result Attack(ICreature creature, IUsableAttackOnCreature item);

    /// <summary>
    ///     Set creature as enemy. If monster can't see creature it will be forgotten
    /// </summary>
    void SetAsEnemy(ICreature actor);

    void GainExperience(long exp);
    void LoseExperience(long exp);
    void AddCondition(ICondition condition);
    void RemoveCondition(ICondition condition);
    void RemoveCondition(ConditionType type);
    bool HasCondition(ConditionType type, out ICondition condition);
    bool HasCondition(ConditionType type);
    void PropagateAttack(AffectedLocation area, CombatDamage damage);
    void OnEnemyAppears(ICombatActor enemy);
    bool IsHostileTo(ICombatActor enemy);
    event StopAttack OnAttackCanceled;
    Result CanAttack(ICombatActor victim);
    Result CanAttack(ITile tile);
    bool ReceiveAttackFrom(IThing enemy, CombatDamage damage);
    void PostAttack(AttackInput attackInput);
    void PreAttack(PreAttackValues preAttackValues);
    bool CanBlock(CombatDamage damage);
    void UpdateBlockCounter(BlockType blockType);
    CombatDamage OnImmunityDefense(CombatDamage damage);
    bool HasImmunity(Immunity immunity);
    void ProcessDamage(IThing enemy, CombatDamage damage);
    void ProcessDamage(IThing enemy, ICombatActor actor, CombatDamageList damageList);
}