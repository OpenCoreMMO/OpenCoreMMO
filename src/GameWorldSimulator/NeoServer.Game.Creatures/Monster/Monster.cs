using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        _damages = new Dictionary<ICreature, ushort>();
        State = MonsterState.Sleeping;
        OnInjured += (enemy, _, damage) => RecordDamage(enemy, damage.Damage);
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

    public ImmutableDictionary<ICreature, ushort> Damages => _damages.ToImmutableDictionary();

    public virtual void Born(Location location)
    {
        _damages.Clear();
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

    public override bool ReceiveAttack(IThing enemy, CombatDamage damage)
    {
        if (this is Summon.Summon { Master: IPlayer })
        {
            return base.ReceiveAttack(enemy, damage);
        }
        
        return enemy is Summon.Summon { Master: IPlayer } or IPlayer && base.ReceiveAttack(enemy, damage);
    }

    public override ushort ArmorRating => Metadata.Armor;
    public override IOutfit Outfit { get; protected set; }
    public override ushort MinimumAttackPower => 0;
    public override bool UsingDistanceWeapon => TargetDistance > 1;
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

        var canSee = CanSee(creature.Location, (int)MapViewPort.MaxClientViewPortX + 1, (int)MapViewPort.MaxClientViewPortX + 1);

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

    public override bool IsThinking() => !IsSleeping;

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
            if (!Cooldowns.Expired(summon.Name)) continue;

            if (summon.Chance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            byte count = 0;
            var foundAliveSummon = _aliveSummons != null && _aliveSummons.TryGetValue(summon.Name, out count);

            if (foundAliveSummon && count >= summon.Max) continue;

            var createdSummon = summonService.Summon(this, summon.Name);
            if (createdSummon is null) continue;

            Cooldowns.Start(summon.Name, (int)summon.Interval);

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
            if (!attack.Cooldown.Expired) continue;

            if (attack.Chance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            if (attack.CombatAttack is null)
            {
                Console.WriteLine($"Combat attack not found for monster: {Name}");
                continue;
            }

            if (attack.CombatAttack.TryAttack(this, enemy, attack.Translate(), out var combatAttack) is false) continue;

            combatAttacks[numberOfSuccessfulAttacks++] = combatAttack;

            attacked = true;

            if (comboChance < GameRandom.Random.Next(0, maxValue: 100) ||
                numberOfSuccessfulAttacks >= maxNumberOfAttacks)
                break; //chance to combo next attack

            comboChance = Math.Max(0, comboChance - 30);
        }

        if (attacked && enemy.Location != Location) TurnTo(enemy);

        if (enemy.IsDead) Targets.RemoveTarget(enemy);

        arrayPool.Return(combatAttacks);
        combatAttacks = combatAttacks[..numberOfSuccessfulAttacks];


        return attacked ? Result.Success : Result.NotPossible;
    }

    public void UpdateLastTargetChance()
    {
        if (!Cooldowns.Expired(CooldownType.TargetChange)) return;
        Cooldowns.Start(CooldownType.TargetChange, Metadata.TargetChance.Interval);
    }

    public void RecordDamage(IThing enemy, ushort damage)
    {
        if (enemy is not ICreature creature) return;
        _damages.AddOrUpdate(creature, oldValue => (ushort)(oldValue + damage));
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

    public override ILoot DropLoot()
    {
        var lootItems = Metadata.Loot?.Drop();

        var enemies = GetLootOwners();

        var loot = new Loot.Loot(lootItems, enemies.ToHashSet());

        return loot;
    }

    private List<ICreature> GetLootOwners()
    {
        var enemies = new HashSet<ICreature>();
        var partyMembers = new List<ICreature>();

        ushort maxDamage = 0;

        foreach (var damage in _damages)
        {
            if (damage.Value > maxDamage)
            {
                enemies.Clear();
                enemies.Add(damage.Key);
                maxDamage = damage.Value;
                continue;
            }

            if (damage.Value == maxDamage) enemies.Add(damage.Key);
        }

        foreach (var enemy in enemies)
            if (enemy is IPlayer player && player.PlayerParty.Party is not null)
                partyMembers.AddRange(player.PlayerParty.Party.Members);

        return !partyMembers.Any() ? enemies.ToList() : enemies.Concat(partyMembers).ToList();
    }

    public override CombatDamage OnImmunityDefense(CombatDamage damage)
    {
        return MonsterDefend.ImmunityDefend(this, damage);
    }

    public override void OnDamage(IThing enemy, CombatDamage damage)
    {
        ReduceHealth(damage);
    }

    protected void ChangeAttackTarget(ICreature creature)
    {
        Follow(creature);
        SetAttackTarget(creature);
        UpdateLastTargetChance();
    }

    #region Summon Event Attachment

    private void AttachToSummonEvents(IMonster monster)
    {
        monster.OnDeath += OnSummonDie;
    }

    private void OnSummonDie(ICombatActor creature, IThing by, ILoot loot)
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