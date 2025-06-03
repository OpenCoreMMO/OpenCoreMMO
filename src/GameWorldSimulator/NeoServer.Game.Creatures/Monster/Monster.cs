using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Combat;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Combat;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Inspection;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures.Monster.Actions;
using NeoServer.Game.Creatures.Monster.Combat;

namespace NeoServer.Game.Creatures.Monster;

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
    private IMonsterCombatAttack[] Attacks => Metadata.Attacks;
    internal ICombatDefense[] Defenses => Metadata.Defenses;
    internal TargetList Targets { get; }
    public override bool CanAttackAnyTarget => Targets.CanAttackAnyTarget;
    public bool HasDistanceAttack => Metadata.HasDistanceAttack;

    public override FindPathParams PathSearchParams
    {
        get
        {
            var fpp = base.PathSearchParams;
            fpp.MaxTargetDist = TargetDistance;
            fpp.KeepDistance = TargetDistance > 1;
            if (TargetDistance <= 1)
                fpp.FullPathSearch = HasDistanceAttack;
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

    public override bool TakeDamage(IThing enemy, CombatDamageList damages)
    {
        if (this is Summon.Summon { Master: IPlayer }) return base.TakeDamage(enemy, damages);

        return enemy is Summon.Summon { Master: IPlayer } or IPlayer && base.TakeDamage(enemy, damages);
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

    public virtual void UpdateState()
    {
        TargetDetector.UpdateTargets(this, MapTool);

        if (!Targets.Any())
        {
            State = Cooldowns.Expired(CooldownType.Awaken) ? MonsterState.Sleeping : MonsterState.LookingForEnemy;
            return;
        }

        if (!CanAttackAnyTarget)
        {
            State = MonsterState.LookingForEnemy;
            return;
        }

        if (Metadata.Flags.TryGetValue(CreatureFlagAttribute.RunOnHealth, out var runOnHealth) &&
            runOnHealth >= HealthPoints)
        {
            State = MonsterState.Escaping;
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

        TargetDetector.UpdateTargets(this, MapTool);
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

    public void Summon(ISummonService summonService)
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

            var createdSummon = summonService.Summon(this, summon.Name);
            if (createdSummon is null) continue;

            Cooldowns.Start(summon);

            AttachToSummonEvents(createdSummon);

            _aliveSummons ??= new Dictionary<string, byte>();

            if (foundAliveSummon) _aliveSummons[summon.Name] = (byte)(count + 1);
            else
                _aliveSummons.TryAdd(summon.Name, 1);
        }
    }

    public override Result OnAttack(ICombatActor enemy, out CombatAttackResult[] combatAttacks)
    {
        combatAttacks = Array.Empty<CombatAttackResult>();
        if (!IsHostile) return Result.Fail(InvalidOperation.AggressorIsNotHostile);

        var arrayPool = ArrayPool<CombatAttackResult>.Shared;

        combatAttacks = arrayPool.Rent(Attacks.Length);

        if (!Attacks.Any()) return Result.NotPossible;

        var attacked = false;

        var maxNumberOfAttacks = (int)Math.Min(3.0, Math.Ceiling(Attacks.Length / 1.5));
        var numberOfSuccessfulAttacks = 0;

        var comboChance = 70;

        foreach (var attack in Attacks)
        {
            if (!Cooldowns.Expired(attack.Id)) continue;

            if (attack.AttackChance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            if (attack.CombatParameter is null)
            {
                Console.WriteLine($"Combat attack not found for monster: {Name}");
                continue;
            }

            // if (attack.CombatAttack.TryAttack(this, enemy, attack.Translate(), out var combatAttack) is false) continue;
            //
            // combatAttacks[numberOfSuccessfulAttacks++] = combatAttack;
            //
            // attacked = true;
            //
            // if (comboChance < GameRandom.Random.Next(0, maxValue: 100) ||
            //     numberOfSuccessfulAttacks >= maxNumberOfAttacks)
            //     break; //chance to combo next attack
            //
            // comboChance = Math.Max(0, comboChance - 30);
        }

        if (attacked && enemy.Location != Location) TurnTo(enemy);

        if (enemy.IsDead) Targets.RemoveTarget(enemy);

        arrayPool.Return(combatAttacks);
        combatAttacks = combatAttacks[..numberOfSuccessfulAttacks];


        return attacked ? Result.Success : Result.NotPossible;
    }

    public IMonsterCombatAttack[] SelectAttacks()
    {
        if (!IsHostile) return [];
        if (Attacks.Length == 0) return [];
        var maxNumberOfAttacks = (int)Math.Min(2, Math.Ceiling(Attacks.Length / 1.5));
        var comboChance = 30;

        Span<IMonsterCombatAttack> selectedAttacks = new IMonsterCombatAttack[2];

        var numberOfAttacks = 0;
        foreach (var attack in Attacks)
        {
            if (numberOfAttacks > maxNumberOfAttacks) break;

            if (!Cooldowns.Expired(attack.Id)) continue;

            if (attack.AttackChance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            if (attack.CombatParameter is null)
            {
                Console.WriteLine($"Combat attack not found for monster: {Name}");
                continue;
            }

            selectedAttacks[numberOfAttacks++] = attack;

            if (comboChance < GameRandom.Random.Next(0, maxValue: 100))
            {
                break;
            }
        }

        return selectedAttacks[..numberOfAttacks].ToArray();
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

        Targets?.Clear();

        StopDefending();
        base.Death(by);
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

    public override Result CanAttack(CombatParameter combatParameter)
    {
        if (!Cooldowns.Expired(combatParameter.CooldownId))
        {
            return Result.Fail(InvalidOperation.CannotAttackThatFast);
        }

        return base.CanAttack(combatParameter);
    }

    #region Summon Event Attachment

    private void AttachToSummonEvents(IMonster monster)
    {
        monster.OnDeath += OnSummonDie;
    }

    private void OnSummonDie(ICombatActor creature, IThing by)
    {
        creature.OnDeath -= OnSummonDie;
        if (!_aliveSummons.TryGetValue(creature.Name, out var count)) return;

        if (count == 1)
        {
            _aliveSummons.Remove(creature.Name);
            return;
        }

        _aliveSummons[creature.Name] = (byte)(count - 1);
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