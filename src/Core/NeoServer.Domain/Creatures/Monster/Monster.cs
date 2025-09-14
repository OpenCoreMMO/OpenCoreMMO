using NeoServer.Domain.Combat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Inspection;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Monster.Actions;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Creatures.Monster;

public class Monster : WalkableMonster, IMonster
{
    // TODO: Organize the variables and properties
    private readonly Dictionary<ICreature, ushort> _damages;
    private Dictionary<string, byte> _aliveSummons;
    private MonsterState _state;

    public Monster(IMonsterType type, IMapTool mapTool, ISpawnPoint spawn) : base(type, mapTool)
    {
        if (type.IsNull()) return;

        Spawn = spawn;
        Direction = spawn?.Direction ?? Direction.North;

        State = MonsterState.Sleeping;
        Targets = new TargetList(this);
    }

    private byte TargetDistance =>
        Metadata.Flags.TryGetValue(CreatureFlagAttribute.TargetDistance, out var targetDistance)
            ? (byte)targetDistance
            : (byte)1;

    protected override string InspectionText => $"{IInspectionTextBuilder.GetArticle(Name)} {Name.ToLower()}.";
    protected override string CloseInspectionText => InspectionText;

    private bool KeepDistance => TargetDistance > 1;
    private MonsterCombatType[] Attacks => Metadata.Attacks;
    internal ICombatDefense[] Defenses => Metadata.Defenses;
    public TargetList Targets { get; }
    public override bool CanAttackAnyTarget => Targets.CanAttackAnyTarget;
    public bool HasDistanceAttack => Metadata.HasDistanceAttack;

    public override FindPathParams PathSearchParams
    {
        get
        {
            var fpp = base.PathSearchParams;
            fpp.FullPathSearch = true;
            fpp.MaxTargetDist = TargetDistance;
            fpp.KeepDistance = TargetDistance > 1;
            return fpp;
        }
    }

    public ushort Defense => Metadata.Defense;

    public MonsterState State
    {
        get => _state;
        private set
        {
            var oldState = _state;
            if (_state == value) return;
            _state = value;
            OnChangedState?.Invoke(this, oldState, value);
        }
    }

    public virtual void Born(Location location)
    {
        ResetHealthPoints();
        SetNewLocation(location);
        State = MonsterState.Sleeping;
        OnWasBorn?.Invoke(this, location);
    }

    public override void OnSpectatorMoved(ICreature spectator)
    {
        if (IsDead) return;
        if (spectator is not ICombatActor target) return;

        if (Targets.HasTarget(spectator))
        {
            Targets.OnTargetMoved(target);
        }

        base.OnSpectatorMoved(spectator);
    }
    
    public override void OnSpectatorLoggedOut(ICreature spectator)
    {
        if (IsDead) return;
        if (spectator is not ICombatActor target) return;
        
        if (Targets.HasTarget(spectator))
        {
            Targets.RemoveTarget(target);
        }
        
        base.OnSpectatorLoggedOut(spectator);
    }

    public override void OnSpectatorDies(ICombatActor spectator)
    {
        if (IsDead) return;

        if (Targets.HasTarget(spectator))
        {
            Targets.RemoveTarget(spectator);
        }

        base.OnSpectatorDies(spectator);
    }

    public void Reborn()
    {
        if (Spawn is null) return;
        Born(Spawn.Location);
    }

    public override int DefendUsingShield(int attack)
    {
        return MonsterDefend.DefendUsingShield(this, attack);
    }

    public override int DefendUsingArmor(int attack)
    {
        return MonsterDefend.DefendUsingArmor(this, attack);
    }

    public override DamageResult TakeDamage(IThing enemy, CombatDamageList damages)
    {
        if (this is Summon.Summon { Master: IPlayer }) return base.TakeDamage(enemy, damages);

        if (enemy is Summon.Summon { Master: IPlayer } or IPlayer or MagicField)
        {
            var damageResult = base.TakeDamage(enemy, damages);
            return damageResult;
        }

        return new DamageResult(damages, false);
    }

    public override ushort ArmorRating => Metadata.Armor;
    public override IOutfit Outfit { get; protected set; }
    public override ushort MinimumAttackPower => 0;
    public override bool UsingDistanceWeapon => TargetDistance > 1;
    public override ushort MaximumAttackPower { get; } = 100;
    public override ushort MaximumElementalAttackPower { get; }
    public ISpawnPoint Spawn { get; }

    public bool IsHostile => Metadata.HasFlag(CreatureFlagAttribute.Hostile);
    public bool IsCurrentTargetUnreachable => Targets.IsCurrentTargetUnreachable;

    public uint Experience => Metadata.Experience;
    public bool IsInCombat => State == MonsterState.InCombat;
    public bool IsSleeping => State == MonsterState.Sleeping;
    public bool Defending { get; private set; }
    public virtual bool IsSummon => false;
    public override bool CanSeeInvisible => HasImmunity(Immunity.Invisibility); //todo: add invisibility flag
    public override bool CanBeSeen => false;

    public override BloodType BloodType => Metadata.Race switch
    {
        Race.Bood => BloodType.Blood,
        Race.Venom => BloodType.Slime,
        Race.Undead => BloodType.None,
        Race.Fire => BloodType.None,
        Race.Energy => BloodType.None,
        _ => BloodType.Blood
    };


    public override void SetAsEnemy(ICreature creature)
    {
        if (creature is not ICombatActor enemy) return;
        if (!(creature is Summon.Summon) && creature is IMonster { IsSummon: false }) return;
        if (creature is Summon.Summon summon && summon.Master.CreatureId == CreatureId) return;

        if (!enemy.CanBeAttacked) return;

        if (creature is IPlayer player && (
                player.Group.FlagIsEnabled(PlayerFlag.IgnoredByMonsters) ||
                player.Group.FlagIsEnabled(PlayerFlag.CannotBeAttacked))) return;

        var canSee = CanSee(creature.Location, (int)MapViewPort.MaxClientViewPortX + 1,
            (int)MapViewPort.MaxClientViewPortX + 1);

        if (State == MonsterState.Sleeping)
            Awake();

        if (IsDead || !canSee)
        {
            Targets.RemoveTarget(creature);
            return;
        }

        Targets.AddTarget(enemy);
    }

    public override bool CanSee(Location pos)
    {
        return base.CanSee(pos, (int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortY, 1);
    }

    public virtual void UpdateState()
    {
        //TargetDetectorService.UpdateTargets(this, MapTool);

        if (!Targets.Any())
        {
            State = Cooldowns.Expired(CooldownType.Awaken) ? MonsterState.Sleeping : MonsterState.LookingForEnemy;
            return;
        }

        if (Metadata.Flags.TryGetValue(CreatureFlagAttribute.RunOnHealth, out var runOnHealth) &&
            runOnHealth >= HealthPoints)
        {
            State = MonsterState.Escaping;
            return;
        }

        if (!CanAttackAnyTarget)
        {
            State = MonsterState.LookingForEnemy;
            return;
        }

        State = MonsterState.InCombat;
    }

    public override bool IsThinking()
    {
        return !IsSleeping;
    }

    public void MoveAroundEnemy()
    {
        if (!Targets.TryGetTarget(AutoAttackTargetId, out var combatTarget)) return;

        if (!IsInPerfectPositionToCombat(combatTarget)) return;

        MoveAroundEnemy(combatTarget);
    }

    public virtual void SelectTargetToAttack()
    {
        if (Attacking && !Cooldowns.Cooldowns[CooldownType.TargetChange].Expired) return;

        //TargetDetectorService.UpdateTargets(this, MapTool);
        var target = Targets.PossibleTargetToAttack;

        if (target is null) return;
        ChangeAttackTarget(target.Creature);
    }

    public void Sleep()
    {
        State = MonsterState.Sleeping;

        StopAttack();
        StopFollowing();
    }

    public void Escape()
    {
        MonsterEscape.Escape(this);
    }

    public void Yell()
    {
        MonsterYell.Yell(this);
    }

    public ushort Defend()
    {
        if (IsDead || !Defenses.Any())
        {
            StopDefending();
            return default;
        }

        Defending = true;

        var defenseIndex = GameRandom.Random.Next(0, maxValue: Defenses.Length);
        var defense = Defenses[defenseIndex];

        if (defense.Chance < GameRandom.Random.Next(1, maxValue: 100))
            return defense.Interval; //can defend but lost his chance

        defense.Defend(this);

        return defense.Interval;
    }

    public void CreateSummon(ISummonService summonService)
    {
        if (IsDead) return;
        if ((_aliveSummons?.Count ?? 0) >= Metadata.MaxSummons) return;

        foreach (var summon in Metadata.Summons)
        {
            if (!Cooldowns.Expired(summon)) continue;

            if (summon.Chance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            byte count = 0;
            var foundAliveSummon = _aliveSummons != null && _aliveSummons.TryGetValue(summon.Name, out count);

            if (foundAliveSummon && count >= summon.Max) continue;

            var createdSummon = summonService.SpamSummon(this, summon.Name);
            if (createdSummon is null) continue;

            Cooldowns.Start(summon);
            
            _aliveSummons ??= new Dictionary<string, byte>();

            if (foundAliveSummon) _aliveSummons[summon.Name] = (byte)(count + 1);
            else
                _aliveSummons.TryAdd(summon.Name, 1);
        }
    }

    public void PostAttack(MonsterCombatType type)
    {
        Cooldowns.Start(type.Id, type.Interval);
    }

    public override Result CanAttack(CombatParameter combatParameter)
    {
        if (combatParameter.DamageType == DamageType.Melee && State == MonsterState.Escaping)
            return Result.Fail(InvalidOperation.NotPossible);

        if (!Cooldowns.Expired(combatParameter.CooldownId)) return Result.Fail(InvalidOperation.CannotAttackThatFast);

        return base.CanAttack(combatParameter);
    }

    public override void AddCondition(ICondition condition)
    {
        switch (condition.Type)
        {
            case ConditionType.Paralyze when HasImmunity(Immunity.Paralysis):
            case ConditionType.Drowning when HasImmunity(Immunity.Drown):
            case ConditionType.Electrified when HasImmunity(Immunity.Energy):
            case ConditionType.Burning when HasImmunity(Immunity.Fire):
            case ConditionType.Drunk when HasImmunity(Immunity.Drunkenness):
            case ConditionType.Poisoned when HasImmunity(Immunity.Earth):
            case ConditionType.Bleeding when HasImmunity(Immunity.Physical):
                return;
            default:
                base.AddCondition(condition);
                break;
        }
    }

    public void UpdateLastTargetChance()
    {
        if (!Cooldowns.Expired(CooldownType.TargetChange)) return;
        Cooldowns.Start(CooldownType.TargetChange, Metadata.TargetChance.Interval);
    }

    protected void Awake()
    {
        State = MonsterState.Awake;
        Cooldowns.Start(CooldownType.Awaken, 10000);
    }

    public bool IsInPerfectPositionToCombat(CombatTarget target)
    {
        if (HasDistanceAttack && target.HasSightClear && !target.CanReachCreature && target.IsInRange(this))
            return true;

        if (KeepDistance)
        {
            if (target.Creature.Location.GetMaxSqmDistance(Location) == TargetDistance)
                return target.CanReachCreature;
        }
        else
        {
            if (target.Creature.Location.GetMaxSqmDistance(Location) <= TargetDistance)
                return target.CanReachCreature;
        }

        return false;
    }

    public override bool HasImmunity(Immunity immunity)
    {
        return (Metadata.Immunities & (ushort)immunity) != 0;
    }

    public override void OnWalkableCreatureDisappear(ICreature creature)
    {
        Targets.RemoveTarget(creature);
        SelectTargetToAttack();
    }

    public void StopDefending()
    {
        Defending = false;
    }

    public override void Death(IThing by)
    {
        if (by is IPlayer player && ReferenceEquals(player.CurrentTarget, this))
            player.StopAttack();

        base.Death(by);
    }

    public override void Dismiss()
    {
        Targets?.Clear();
        StopDefending();
        base.Dismiss();
    }

    public override CombatDamage OnImmunityDefense(CombatDamage damage)
    {
        return MonsterDefend.ImmunityDefend(this, damage);
    }

    public override void OnDamage(IThing enemy, CombatDamageList damages)
    {
        ReduceHealth(damages.TotalDamage.HealthDamage);
    }

    protected void ChangeAttackTarget(ICreature creature)
    {
        Follow(creature);
        SetAttackTarget(creature);
        UpdateLastTargetChance();
    }

    #region Summon Event Attachment
    public override void OnSummonDie(Summon.Summon summon)
    {
        if (summon is null) return;
        if (_aliveSummons is null || !_aliveSummons.TryGetValue(summon.Name, out var count)) return;

        if (count == 1)
        {
            _aliveSummons.Remove(summon.Name);
            return;
        }

        _aliveSummons[summon.Name] = (byte)(count - 1);
    }

    public override bool IsHostileTo(ICombatActor enemy)
    {
        return enemy is not IMonster && IsHostile;
    }

    #endregion

    #region Events

    public event Born OnWasBorn;
    public event MonsterChangeState OnChangedState;

    #endregion
}