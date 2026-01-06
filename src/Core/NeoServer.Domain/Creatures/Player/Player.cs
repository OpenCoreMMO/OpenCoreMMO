using System.Text;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Creatures.Players;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Common;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Domain.Creatures.Models;
using NeoServer.Domain.Creatures.Models.Bases;
using NeoServer.Domain.Creatures.Npcs;
using NeoServer.Domain.Creatures.Player.Container;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Items.Items.UsableItems;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Creatures.Player;

public class Player : CombatActor, IPlayer
{
    private const int KNOWN_CREATURE_LIMIT = 250; //todo: for version 8.60


    private byte _soulPoints;

    public Player(
        uint id,
        string characterName,
        ChaseMode chaseMode,
        uint capacity,
        uint healthPoints,
        uint maxHealthPoints,
        Vocation.Vocation vocation,
        Group group,
        Gender gender,
        bool online,
        uint mana,
        uint maxMana,
        FightMode fightMode,
        byte soulPoints,
        byte soulMax,
        IDictionary<SkillType, Skill> skills,
        IDictionary<uint, int> storages,
        ushort staminaMinutes,
        Outfit.Outfit outfit,
        ushort speed,
        Location location,
        IMapTool mapTool, ITown town)
        : base(
            new CreatureType(
                characterName,
                string.Empty,
                healthPoints,
                maxHealthPoints,
                speed,
                new Dictionary<LookType, ushort> { { LookType.Corpse, 3058 } }),
            mapTool,
            outfit,
            healthPoints)
    {
        Id = id;
        CharacterName = characterName;
        ChaseMode = chaseMode;
        Skills = skills;
        Storages = storages;
        Vocation = vocation;
        Group = group;
        Gender = gender;
        Online = online;
        Mana = mana;
        MaxMana = maxMana;
        FightMode = fightMode;
        MaxSoulPoints = soulMax;
        SoulPoints = soulPoints;
        StaminaMinutes = staminaMinutes;
        Outfit = outfit;
        Speed = speed == 0 ? RawSpeed : speed;
        Inventory = new Inventory.Inventory(this, new Dictionary<Slot, (IItem Item, ushort Id)>());

        TotalCapacity = Group.FlagIsEnabled(PlayerFlag.HasInfiniteCapacity) ? uint.MaxValue : capacity;

        Vip = new Vip(this);
        Channels = new PlayerChannel(this);
        PlayerParty = new PlayerParty(this);
        PlayerHand = new PlayerHand(this);

        SetNewLocation(location);
        Town = town;

        Containers = new PlayerContainerList(this);

        PlayerSkull ??= new PlayerSkull(this);

        KnownCreatures = new Dictionary<uint, long>(); //todo

        foreach (var skill in Skills.Values)
        {
            skill.OnAdvance += OnLevelAdvance;
            skill.OnRegress += OnLevelRegress;
            skill.OnIncreaseSkillPoints += skill => OnGainedSkillPoint?.Invoke(this, skill);
        }
    }

    protected override string CloseInspectionText => InspectionText;

    protected override string InspectionText =>
        $"{Name} (Level {Level}). {GenderPronoun} {Vocation.InspectText}. {Guild?.InspectionText(this)} {PlayerParty?.Party?.InspectionText(this)}";

    public string CharacterName { get; }
    public Dictionary<uint, long> KnownCreatures { get; }
    public bool Online { get; private set; }

    public int DefenseFactor => FightMode switch
    {
        FightMode.Attack => 5,
        FightMode.Balanced => 7,
        FightMode.Defense => 10,
        _ => 7
    };

    public PlayerHand PlayerHand { get; }

    public uint IdleTime { get; private set; }
    public List<RegenerationBonus> RegenerationBonusList { get; private set; } = new();

    public uint LoggedOutTotalMinutes => !LastLogIn.HasValue || !LastLogOut.HasValue
        ? 0
        : (uint)(LastLogIn.Value - LastLogOut.Value).TotalMinutes;

    public long LastTimeExperienceGain { get; private set; }

    public override ushort RawSpeed =>
        Group.FlagIsEnabled(PlayerFlag.SetMaxSpeed) ? ushort.MaxValue : (ushort)(220 + 2 * (Level - 1));

    public float DamageFactor => FightMode switch
    {
        FightMode.Attack => 1,
        FightMode.Balanced => 0.75f,
        FightMode.Defense => 0.5f,
        _ => 0.75f
    };

    public bool IsPacified => Conditions.ContainsKey(ConditionType.Pacified);

    public IDictionary<SkillType, Skill> Skills { get; }

    /// <summary>
    ///     Gender pronoun: He/She
    /// </summary>
    public string GenderPronoun => Gender == Gender.Male ? "He" : "She";

    public Gender Gender { get; set; }
    public int PremiumDays { get; init; }
    public bool HasPremiumTime => PremiumDays > 0 || Group.FlagIsEnabled(PlayerFlag.IsAlwaysPremium);
    public ITown Town { get; set; }
    public IVip Vip { get; }
    public override Outfit.Outfit Outfit { get; protected set; }
    public Vocation.Vocation Vocation { get; }
    public Group Group { get; set; }
    public PlayerChannel Channels { get; set; }
    public PlayerParty PlayerParty { get; set; }
    public IBank Bank { get; private set; }
    public ulong BankAmount => Bank?.Amount ?? 0;

    public int NumberOfUnjustifiedKillsLastDay { get; private set; }
    public int NumberOfUnjustifiedKillsLastWeek { get; private set; }
    public int NumberOfUnjustifiedKillsLastMonth { get; private set; }

    public ulong GetTotalMoney(ICoinTypeStore coinTypeStore)
    {
        return BankAmount + Inventory.GetTotalMoney(coinTypeStore);
    }

    public void LoadBank(ulong amount)
    {
        Bank ??= new Bank(amount);
    }

    public uint AccountId { get; init; }
    public int WorldId { get; init; }
    public PlayerContainerList Containers { get; }
    public bool HasDepotOpened => Containers.HasAnyDepotOpened;
    public IShopperNpc TradingWithNpc { get; private set; }
    public ChaseMode ChaseMode { get; private set; }
    public uint TotalCapacity { get; private set; }
    public ushort Level => (ushort)(Skills.TryGetValue(SkillType.Level, out var level) ? level?.Level ?? 1 : 1);
    public ushort MagicLevel => (ushort)(Skills.TryGetValue(SkillType.Magic, out var level) ? level?.Level ?? 1 : 1);
    public uint Mana { get; private set; }
    public uint ManaSpent { get; private set; }
    public uint MaxMana { get; private set; }
    public FightMode FightMode { get; private set; }
    public PlayerSkull PlayerSkull { get; set; }
    public Skull Skull => PlayerSkull.Skull;
    public DateTime? SkullEndsAt => PlayerSkull.SkullEndsAt;
    public DateTime? LastLogIn { get; private set; }
    public required DateTime? LastLogOut { get; set; }

    public bool Shopping => TradingWithNpc is not null;

    public byte SoulPoints
    {
        get => _soulPoints;
        private set => _soulPoints = value > MaxSoulPoints ? MaxSoulPoints : value;
    }

    public byte MaxSoulPoints { get; }

    public IInventory Inventory { get; private set; }

    public uint Experience
    {
        get
        {
            if (Skills.TryGetValue(SkillType.Level, out var skill)) return (uint)skill.Count;
            return 0;
        }
    }

    public bool IsPromoted => Vocation.IsPromotion;

    public void AddInventory(IInventory inventory)
    {
        Inventory = inventory;
    }

    public byte LevelPercent => GetSkillPercent(SkillType.Level);
    public override bool CanBeAttacked => !Group.FlagIsEnabled(PlayerFlag.CannotBeAttacked) && base.CanBeAttacked;

    public override void GainExperience(long experience)
    {
        if (experience == 0) return;

        if (Group.FlagIsEnabled(PlayerFlag.NotGainExperience)) return;

        if (!IgnoreStamina)
        {
            experience = ApplyStaminaEffectOnExperienceGain(experience);

            var elapsedSecondsSinceLastGain =
                (DateTime.UtcNow.Ticks - LastTimeExperienceGain) / TimeSpan.TicksPerSecond;

            if (elapsedSecondsSinceLastGain >= 60) ConsumeStamina();
        }

        LastTimeExperienceGain = DateTime.UtcNow.Ticks;

        IncreaseSkillCounter(SkillType.Level, experience);
        base.GainExperience(experience);
    }

    public override void LoseExperience(long exp)
    {
        if (exp == 0) return;

        DecreaseSkillCounter(SkillType.Level, exp);
        base.LoseExperience(exp);
    }

    public void RegenerateStamina()
    {
        if (LastLogOut is null || LastLogIn is null || IgnoreStamina) return;

        if (LoggedOutTotalMinutes <= 10) return;

        var multiplier = GameConstants.STAMINA_REGENERATION_EACH_MINUTES;

        if (HasStaminaBonus) multiplier *= 2;

        var minutesRecoveredSinceLoggedIn = Math.Abs((decimal)LoggedOutTotalMinutes / multiplier);

        RecoverStamina((ushort)minutesRecoveredSinceLoggedIn);
    }

    public override decimal AttackSpeed => Vocation.AttackSpeed == 0 ? base.AttackSpeed : Vocation.AttackSpeed;

    public virtual bool CannotLogout => !(Tile?.ProtectionZone ?? false) && IsLogoutBlocked;

    public void SetProtectionZoneBlock()
    {
        if (IsPacified) return;

        if (HasCondition(ConditionType.ProtectionZoneBlock, out var condition))
        {
            condition.Start(this);
            return;
        }

        //protection zone block is persistent, this will be removed elsewhere
        AddCondition(new Condition(ConditionType.ProtectionZoneBlock, 0));
    }

    public void RemoveProtectionZoneBlock()
    {
        RemoveCondition(ConditionType.ProtectionZoneBlock);
    }

    public virtual bool IsProtectionZoneBlocked => HasCondition(ConditionType.ProtectionZoneBlock);

    public bool HasSkull => Skull is not Skull.None;

    public SkillType SkillInUse
    {
        get
        {
            if (Inventory.Weapon is { } weapon)
                return weapon.Type switch
                {
                    WeaponType.Club => SkillType.Club,
                    WeaponType.Sword => SkillType.Sword,
                    WeaponType.Axe => SkillType.Axe,
                    WeaponType.Ammunition => SkillType.Distance,
                    WeaponType.Distance => SkillType.Distance,
                    WeaponType.Magical => SkillType.Magic,
                    _ => SkillType.Fist
                };
            return SkillType.Fist;
        }
    }

    public ushort CalculateAttackPower(float attackRate, ushort attack)
    {
        var damageMultiplier = SkillInUse switch
        {
            SkillType.Distance => Vocation.Formula?.DistDamage ?? 1f,
            SkillType.Magic => 1f,
            _ => Vocation.Formula?.MeleeDamage ?? 1f
        };
        return (ushort)(attackRate * DamageFactor * attack * Skills[SkillInUse].Level + Level / 5 * damageMultiplier);
    }

    public uint Id { get; }
    public override ushort MinimumAttackPower => Inventory.Weapon?.MinHitChance ?? (ushort)(Level / 5);
    public override ushort MaximumAttackPower => CalculateTotalAttack(Inventory.TotalAttack);

    public override ushort ArmorRating => Inventory.TotalArmor;
    public PvpSecureMode SecureMode { get; private set; }

    public float FreeCapacity => Group.FlagIsEnabled(PlayerFlag.HasInfiniteCapacity)
        ? float.MaxValue
        : TotalCapacity - Inventory.TotalWeight;

    public override bool UsingDistanceWeapon => Inventory.Weapon is IDistanceWeapon;
    public bool Recovering => HasCondition(ConditionType.Regeneration);
    public override bool CanSeeInvisible => Group.FlagIsEnabled(PlayerFlag.CanSenseInvisibility);
    public override bool CanBeSeen => Group.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);
    public virtual bool CanSeeInspectionDetails => Group.Access;

    public override ushort MaximumElementalAttackPower =>
        CalculateTotalAttack(Inventory.TotalElementalAttack.AttackPower, true);

    public override void PreAttack(CombatContext combatContext)
    {
        if (Inventory.Weapon is IDistanceWeapon && !combatContext.InfiniteAmmo) Inventory.Ammo?.Reduce();

        if (Inventory.Weapon is ThrowableWeapon { ShouldBreak: true } throwableDistanceWeapon &&
            !combatContext.InfiniteThrowingWeapon)
            throwableDistanceWeapon.Reduce();

        base.PreAttack(combatContext);
    }

    public ushort GetRawSkillLevel(SkillType skillType)
    {
        var hasSkill = Skills.TryGetValue(skillType, out var skill);
        var skillLevel = hasSkill ? skill.Level : 1;
        return (ushort)Math.Max(0, skillLevel);
    }

    public ushort GetSkillLevel(SkillType skillType)
    {
        var hasSkill = Skills.TryGetValue(skillType, out var skill);
        var skillLevel = hasSkill ? skill.Level : 1;
        var skillBonus = skill?.Bonus ?? 0;
        var totalSkill = skillLevel + skillBonus;
        return (ushort)Math.Max(0, totalSkill);
    }

    public byte GetSkillTries(SkillType skillType)
    {
        return (byte)(Skills.TryGetValue(skillType, out var skill) ? skill.Count : 0);
    }

    public sbyte GetSkillBonus(SkillType skill)
    {
        return Skills[skill].Bonus;
    }

    public void AddSkillBonus(SkillType skillType, sbyte increase)
    {
        if (increase == 0) return;
        if (Skills is null) return;
        if (!Skills.TryGetValue(skillType, out _))
            Skills.Add(skillType, new Skill(skillType, 1, 1)); //todo: review those skill values

        Skills[skillType]?.AddBonus(increase);
        OnAddedSkillBonus?.Invoke(this, skillType, increase);
    }

    public void RemoveSkillBonus(SkillType skillType, sbyte decrease)
    {
        if (decrease == 0) return;

        Skills[skillType]?.RemoveBonus(decrease);
        OnRemovedSkillBonus?.Invoke(this, skillType, decrease);
    }

    public byte GetSkillPercent(SkillType skill)
    {
        var rate = Creatures.Player.Vocation.Vocation.DefaultSkillMultiplier;
        Vocation.Skills?.TryGetValue(skill, out rate);
        return (byte)Math.Clamp(Skills[skill].GetPercentage(rate), 0, 100);
    }

    public bool KnowsCreatureWithId(uint creatureId)
    {
        return KnownCreatures.ContainsKey(creatureId);
    }

    public void AddKnownCreature(uint creatureId)
    {
        KnownCreatures.TryAdd(creatureId, DateTime.UtcNow.Ticks);
    }

    public uint ChooseToRemoveFromKnownSet()
    {
        if (KnownCreatures.Count <= KNOWN_CREATURE_LIMIT) return uint.MinValue; // 0

        // if the buffer is full we need to choose a creature to remove.
        foreach (var candidate in
                 KnownCreatures.OrderBy(kvp => kvp.Value)
                     .ToList())
        {
            CreatureGameInstance.Instance.TryGetCreature(candidate.Key, out var creature);

            if (CanSee(creature)) continue;

            if (KnownCreatures.Remove(candidate.Key)) return candidate.Key;
        }

        // Bad situation. Let's just remove the first valid occurrence.
        foreach (var candidate in
                 KnownCreatures.OrderBy(kvp => kvp.Value)
                     .ToList())
            if (KnownCreatures.Remove(candidate.Key))
                return candidate.Key;
        return uint.MinValue; // 0
    }

    public override void OnMoved(IDynamicTile fromTile, IDynamicTile toTile, ICylinderSpectator[] spectators)
    {
        if (IsTargetLost())
        {
            StopAttack();
            OperationFailService.Send(this, InvalidOperation.TargetLost);
        }

        TogglePacifiedCondition(fromTile, toTile);
        Containers.CloseDistantContainers();
        base.OnMoved(fromTile, toTile, spectators);

        EventAggregator.Invoke(new PlayerWalkEvent(this, Direction));
    }

    public override void OnSpectatorMoved(ICreature spectator)
    {
        if (spectator is not ICombatActor target) return;
        if (target.Equals(CurrentTarget)) HandleTargetLost();

        base.OnSpectatorMoved(spectator);
    }

    public override void OnSpectatorChangedVisibility(ICreature spectator)
    {
        if (spectator is IMonster && spectator.CreatureId == Following.CreatureId && spectator.IsInvisible)
        {
            StopFollowing();
        }
        
        base.OnSpectatorChangedVisibility(spectator);
    }

    public override void OnSpectatorDies(ICombatActor spectator)
    {
        if (spectator.Equals(CurrentTarget)) HandleTargetLost();

        base.OnSpectatorDies(spectator);
    }

    public override bool CanSee(ICreature otherCreature)
    {
        if (otherCreature is null) return false;

        if (!otherCreature.IsInvisible ||
            (otherCreature is IPlayer && otherCreature.CanBeSeen) ||
            CanSeeInvisible)
            return true;

        return CanSee(otherCreature.Location);
    }

    public override bool CanSee(Location pos)
    {
        return base.CanSee(pos, (int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortY, 1);
    }

    public override void TurnInvisible()
    {
        SetTemporaryOutfit(0, 0, 0, 0, 0, 0);
        base.TurnInvisible();
    }

    public override void TurnVisible()
    {
        BackToOldOutfit();
        base.TurnVisible();
    }

    public override void SetAsEnemy(ICreature creature)
    {
        if (creature is not IMonster) return;
        SetLogoutBlock();
    }

    public void StopShopping()
    {
        TradingWithNpc?.StopSellingToCustomer(this);
        TradingWithNpc = null;
    }

    public void StartShopping(IShopperNpc npc)
    {
        TradingWithNpc = npc;
    }

    public void ChangeFightMode(FightMode mode)
    {
        FightMode = mode;
    }

    public void ChangeChaseMode(ChaseMode mode)
    {
        var oldChaseMode = ChaseMode;
        ChaseMode = mode;

        if (ChaseMode == ChaseMode.Follow && CurrentTarget is not null)
        {
            Follow(CurrentTarget as IWalkableCreature, PathSearchParams);
            return;
        }

        StopFollowing();

        OnChangedChaseMode?.Invoke(this, oldChaseMode, mode);
    }

    public void ChangeSecureMode(PvpSecureMode mode)
    {
        SecureMode = mode;
    }

    public override int DefendUsingShield(int attack)
    {
        var defense = Inventory.TotalDefense * Skills[SkillType.Shielding].Level *
            (DefenseFactor / 100d) - attack / 100d * ArmorRating * (Vocation.Formula?.Defense ?? 1f);

        var resultDamage = Math.Max(0, (int)(attack - defense));
        if (resultDamage <= 0) IncreaseSkillCounter(SkillType.Shielding, 1);
        return resultDamage;
    }

    public override int DefendUsingArmor(int damage)
    {
        switch (ArmorRating)
        {
            case > 3:
            {
                var min = ArmorRating / 2 * (Vocation.Formula?.Armor ?? 1f);
                var max = (ArmorRating / 2 * 2 - 1) * (Vocation.Formula?.Armor ?? 1f);
                damage -= (ushort)GameRandom.Random.NextInRange(min, max);
                break;
            }
            case > 0:
                --damage;
                break;
        }

        return damage;
    }

    public void SendMessageTo(ISociableCreature to, SpeechType speechType, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        OnSentMessage?.Invoke(this, to, speechType, message);
    }

    public void PostSpellCast(ISpell spell)
    {
        const SpeechType talkType = SpeechType.MonsterSay;

        if (!Group.FlagIsEnabled(PlayerFlag.HasInfiniteMana)) DecreaseMana(spell.ManaConsumption);

        if (!Group.FlagIsEnabled(PlayerFlag.HasInfiniteSoul)) ConsumeSoul(spell.SoulConsumption);

        UpdateManaSpent(spell.ManaConsumption);

        StartCooldown(spell);

        if (!spell.ShouldSay) return;

        if (!string.IsNullOrWhiteSpace(spell.Words)) Say(spell.Words, talkType);
    }

    public void Yell(string message, YellConfiguration yellSettings)
    {
        message = message.ToUpper();
        if (Group.FlagIsEnabled(PlayerFlag.IgnoreYellCheck))
        {
            base.Yell(message);
            return;
        }

        if (!CooldownHasExpired(CooldownType.Yell))
        {
            OperationFailService.Send(this, InvalidOperation.Exhausted);
            return;
        }

        var minLevel = yellSettings?.YellMinimumLevel ?? 2;
        var allowedWhenPremium = yellSettings?.YellAllowedPremium ?? true;

        if (Level < minLevel)
        {
            var error = new StringBuilder($"You are not allowed to yell until you are level {minLevel}");

            if (allowedWhenPremium && HasPremiumTime)
            {
                base.Yell(message);
                Cooldowns.Start(CooldownType.Yell, 30_000); // 30 seconds cooldown
                return;
            }

            error.Append(" or have a premium account");

            OperationFailService.Send(this, error.ToString());

            return;
        }

        base.Yell(message);
        Cooldowns.Start(CooldownType.Yell,
            (uint)(yellSettings?.YellCooldownSeconds * 1000 ?? 30_000)); // 30 seconds cooldown
    }

    public void StartCooldown(CooldownType cooldownType, uint cooldownTime)
    {
        Cooldowns.Start(cooldownType, cooldownTime);
    }

    public void UpdateManaSpent(uint manaCost)
    {
        Skills.TryGetValue(SkillType.Magic, out var currentMagicLevel);

        ManaSpent += manaCost;
        if (manaCost > 0 && !Group.FlagIsEnabled(PlayerFlag.NotGainSkill))
            IncreaseSkillCounter(SkillType.Magic, manaCost);

        Skills.TryGetValue(SkillType.Magic, out var updatedMagicLevel);

        if (currentMagicLevel != updatedMagicLevel)
            ManaSpent = 0;
    }

    public bool HasEnoughSoul(ushort soul)
    {
        return SoulPoints >= soul;
    }

    public bool HasEnoughMana(uint mana)
    {
        return Mana >= mana;
    }

    public void DecreaseMana(uint mana)
    {
        if (mana == 0) return;
        if (!HasEnoughMana(mana)) return;

        Mana -= mana;
        OnStatusChanged?.Invoke(this);
    }

    public void ConsumeSoul(ushort soul)
    {
        if (soul == 0) return;
        if (!HasEnoughSoul(soul)) return;

        Mana -= soul;
        OnStatusChanged?.Invoke(this);
    }

    public bool HasEnoughLevel(ushort level)
    {
        return Level >= level;
    }

    public void LookAt(ITile tile)
    {
        var isClose = Location.IsNextTo(tile.Location);
        if (tile.TopCreatureOnStack is null && tile.TopDownItemOnStack is null) return;

        IThing thing = tile.TopCreatureOnStack is null ? tile.TopDownItemOnStack : tile.TopCreatureOnStack;
        OnLookedAt?.Invoke(this, thing, isClose);
    }

    public void LookAt(byte containerId, sbyte containerSlot)
    {
        if (Containers[containerId][containerSlot] is not IThing thing) return;
        OnLookedAt?.Invoke(this, thing, true);
    }

    public void LookAt(Slot slot)
    {
        if (Inventory[slot] is not IThing thing) return;
        OnLookedAt?.Invoke(this, thing, true);
    }

    public void Read(IReadable readable)
    {
        EventAggregator.Invoke(new PlayerReadTextEvent(this, readable, readable.Text));
    }

    public void Write(IReadable readable, string text)
    {
        var result = readable.Write(text, this);
        if (result.Failed)
        {
            OperationFailService.Send(CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        OnWroteText?.Invoke(this, readable, readable.Text);
    }

    public bool Logout(bool forced = false)
    {
        if (CannotLogout && !forced)
        {
            OperationFailService.Send(CreatureId, "You may not logout during or immediately after a fight");
            return false;
        }

        StopAttack();
        StopFollowing();
        StopWalking();
        Containers.CloseAll();
        ChangeOnlineStatus(false);
        PlayerParty.LeaveParty();
        PlayerParty.RejectAllInvites();
        PlayerSkull.RemoveYellowSkull();
        LastLogOut = DateTime.UtcNow;

        var summonsCopy = Summons.ToList();
        foreach (var summon in summonsCopy) summon.OnMasterLogout();

        EventAggregator.Invoke(new PlayerLoggedOutEvent(this));

        return true;
    }

    public bool Login()
    {
        StopAttack();
        StopFollowing();
        StopWalking();
        ChangeOnlineStatus(true);
        TogglePacifiedCondition(null, Tile);
        KnownCreatures.Clear();

        LastLogIn = DateTime.UtcNow;
        RegenerateStamina();

        EventAggregator.Invoke(new PlayerLoggedInEvent(this));
        return true;
    }

    public void IncreaseMana(uint increasing)
    {
        if (Group.FlagIsEnabled(PlayerFlag.NotGainMana)) return;

        if (increasing <= 0) return;

        if (Mana == MaxMana) return;

        Mana = Mana + increasing >= MaxMana ? MaxMana : Mana + increasing;
        OnStatusChanged?.Invoke(this);
    }

    public override void Heal(ushort increasing, ICreature healedBy)
    {
        if (Group.FlagIsEnabled(PlayerFlag.NotGainHealth)) return;

        base.Heal(increasing, healedBy);
    }

    public void Recover()
    {
        if (!Recovering) return;

        if (Cooldowns.Expired(CooldownType.HealthRecovery)) Heal(Vocation.GainHpAmount, this);
        if (Cooldowns.Expired(CooldownType.ManaRecovery)) IncreaseMana(Vocation.GainManaAmount);
        if (Cooldowns.Expired(CooldownType.SoulRecovery)) HealSoul(1);

        foreach (var regenerationBonus in RegenerationBonusList)
        {
            if (!Cooldowns.Expired(regenerationBonus.Id)) continue;

            switch (regenerationBonus.Type)
            {
                case RegenerationType.Health:
                    Heal(regenerationBonus.Gain, this);
                    break;
                case RegenerationType.Mana:
                    IncreaseMana(regenerationBonus.Gain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //todo: start these cooldowns when player logs in
        Cooldowns.Start(CooldownType.HealthRecovery, (uint)Vocation.GainHpTicks * 1000);
        Cooldowns.Start(CooldownType.ManaRecovery, (uint)Vocation.GainManaTicks * 1000);
        Cooldowns.Start(CooldownType.SoulRecovery, (uint)Vocation.GainSoulTicks * 1000);

        foreach (var regenerationBonus in RegenerationBonusList)
            Cooldowns.Start(regenerationBonus.Id, (uint)regenerationBonus.Ticks);
    }

    public void AddRegenerationBonus(RegenerationBonus regenerationBonus)
    {
        RegenerationBonusList ??= [];
        RegenerationBonusList.Add(regenerationBonus);
    }

    public void RemoveRegenerationBonus(RegenerationBonus regenerationBonus)
    {
        RegenerationBonusList ??= [];
        RegenerationBonusList.Remove(regenerationBonus);
    }

    public void Use(IThing item)
    {
        if (!item.IsCloseTo(this)) return;

        item.Use(this);
    }

    public void Use(IContainer item, byte openAtIndex)
    {
        if (!item.IsCloseTo(this)) return;

        item.Use(this, openAtIndex);
    }

    public Result Use(IUsableOn item, ICreature onCreature)
    {
        var canUseItem = CanUseItem(item, onCreature.Location);
        if (canUseItem.Failed) return canUseItem;

        var itemUsed = false;

        if (onCreature is ICombatActor enemy)
            switch (item)
            {
                case IUsableOnCreature usableOnCreature:
                    usableOnCreature.Use(this, onCreature);
                    itemUsed = true;
                    break;
                case IUsableOnTile useableOnTile:
                    itemUsed = useableOnTile.Use(this, onCreature.Tile);
                    break;
                case IUsableOnItem useableOnItem:
                    itemUsed = useableOnItem.Use(this, onCreature.Tile.TopDownItemOnStack);
                    break;
            }

        if (itemUsed)
        {
            OnUsedItem?.Invoke(this, onCreature, item);
            Cooldowns.Start(CooldownType.UseItem, (uint)item.CooldownTime);
            return Result.Success;
        }

        OperationFailService.Send(CreatureId, TextConstants.NOT_POSSIBLE);
        return Result.Fail(InvalidOperation.NotPossible);
    }

    public Result Use(IUsableOn item, IItem onItem)
    {
        var canUseItem = CanUseItem(item, onItem.Location);
        if (canUseItem.Failed) return canUseItem;

        if (item is not IUsableOnItem usableOnItem) return Result.Fail(InvalidOperation.CannotUseSpells);

        usableOnItem.Use(this, onItem);
        OnUsedItem?.Invoke(this, onItem, item);
        Cooldowns.Start(CooldownType.UseItem, 1000);

        return Result.Success;
    }

    public Result Use(IUsableOn item, ITile targetTile)
    {
        if (!Cooldowns.Expired(CooldownType.UseItem))
        {
            OnExhausted?.Invoke(this);
            return Result.NotPossible;
        }

        if (!item.IsCloseTo(this)) return Result.NotPossible;

        if (item is IEquipmentRequirement requirement && !requirement.CanBeUsed(this))
        {
            OperationFailService.Send(CreatureId, requirement.ValidationError);
            return Result.NotPossible;
        }

        if (targetTile.TopDownItemOnStack is not { } onItem) return Result.NotPossible;

        var result = item switch
        {
            IUsableOnTile usableOnTile => usableOnTile.Use(this, targetTile),
            IUsableOnItem usableOnItem => usableOnItem.Use(this, onItem),
            _ => false
        };

        if (result) OnUsedItem?.Invoke(this, onItem, item);
        Cooldowns.Start(CooldownType.UseItem, 1000);

        return Result.Success;
    }

    public bool Feed(int duration)
    {
        var regenerationMs = (uint)duration * 1000;
        const uint maxRegenerationTime = (uint)1200 * 1000; //20 minutes

        if (Conditions.TryGetValue(ConditionType.Regeneration, out var condition))
        {
            if (condition.RemainingTime + regenerationMs >=
                maxRegenerationTime) //todo: this number should be configurable
            {
                OperationFailService.Send(CreatureId, TextConstants.YOU_ARE_FULL);
                return false;
            }

            condition.Extend(regenerationMs, maxRegenerationTime);
        }
        else
        {
            RemoveHungry();
            AddCondition(new Condition(ConditionType.Regeneration, regenerationMs, SetAsHungry));
        }

        return true;
    }

    public bool Feed(Food food)
    {
        if (food is null) return false;
        return Feed(food.Duration);
    }

    public void SetAsHungry()
    {
        RemoveCondition(ConditionType.Regeneration);
        AddCondition(new Condition(ConditionType.Hungry, uint.MaxValue));
    }

    public bool IsManaShieldEnabled => HasCondition(ConditionType.ManaShield);

    public void EnableManaShield(uint duration)
    {
        AddCondition(new Condition(ConditionType.ManaShield, duration,
            () => { RemoveCondition(ConditionType.ManaShield); }));
    }

    public void EnableManaShield()
    {
        AddCondition(new Condition(ConditionType.ManaShield));
    }

    public void DisableManaShield()
    {
        RemoveCondition(ConditionType.ManaShield);
    }

    public Result<OperationResultList<IItem>> PickItemFromGround(IItem item, ITile tile, byte amount = 1)
    {
        return PlayerHand.PickItemFromGround(item, tile, amount);
    }

    public Result<OperationResultList<IItem>> MoveItem(IItem item, IHasItem source, IHasItem destination, byte amount,
        byte fromPosition,
        byte? toPosition)
    {
        return PlayerHand.Move(item, source, destination, amount, fromPosition, toPosition);
    }

    public override Result SetAttackTarget(ICreature target)
    {
        if (target is IPlayer && SecureMode is PvpSecureMode.PvPDisabled)
        {
            StopAttack(true);
            OperationFailService.Send(this, InvalidOperation.AdjustCombatSettingsToAttackPlayer);
            return Result.Fail(InvalidOperation.AdjustCombatSettingsToAttackPlayer);
        }

        if (target.IsInvisible)
        {
            StopAttack();
            return new Result(InvalidOperation.AttackTargetIsInvisible);
        }

        var result = base.SetAttackTarget(target);
        if (result.Failed) return result;

        if (target.CreatureId != 0 && ChaseMode == ChaseMode.Follow) Follow(target, PathSearchParams);

        return result;
    }

    public void Hear(ICreature from, SpeechType speechType, string message)
    {
        if (from is null || speechType == SpeechType.None || string.IsNullOrWhiteSpace(message)) return;

        OnHear?.Invoke(from, this, speechType, message);
    }

    public void ReceivePayment(IEnumerable<IItem> coins, ulong total)
    {
        if (CanReceiveInCashPayment(coins))
        {
            foreach (var coin in coins) Inventory.BackpackSlot.AddItem(coin, true);

            return;
        }

        Bank?.Credit(total);
    }

    public virtual void WithdrawFromBank(ulong amount)
    {
        if (BankAmount >= amount) Bank?.Debit(amount);
    }

    public bool CanReceiveInCashPayment(IEnumerable<IItem> coins)
    {
        var totalWeight = coins.Sum(x => x is ICumulative cumulative ? cumulative.Weight : 0);
        var totalFreeSlots = Inventory.BackpackSlot?.TotalOfFreeSlots ?? 0;

        return !(totalWeight > FreeCapacity) && totalFreeSlots >= coins.Count();
    }

    public void ReceivePurchasedItems(INpc from, SaleContract saleContract, params IItem[] items)
    {
        if (items is null) return;

        var possibleAmountOnInventory = saleContract.PossibleAmountOnInventory;

        foreach (var item in items)
        {
            if (item is null) continue;

            if (possibleAmountOnInventory > 0)
            {
                possibleAmountOnInventory = (uint)Math.Max(0, (int)possibleAmountOnInventory - item.Amount);
                var result = Inventory.AddItem(item);
                if (result.Succeeded)
                {
                    if (!(result.Value?.HasAnyOperation ?? false)) continue;
                    if (result.Value.Operations[0].Item2 != Operation.Removed) continue;
                }
            }

            Inventory.BackpackSlot?.AddItem(item, true);
        }
    }

    public override bool IsHostileTo(ICombatActor enemy)
    {
        return enemy is not IPlayer;
    }

    public void PostAttack(CombatParameter combatParameter, IThing target, CombatResult combatResult)
    {
        SetLogoutBlock();

        if (target is IPlayer) SetProtectionZoneBlock();

        if (!combatParameter.UsingWeapon) return;

        Cooldowns.Start(CooldownType.WeaponAttack, (uint)AttackSpeed);

        if (combatResult.TotalDamage > 0 &&
            SkillInUse != SkillType.Magic) //magic skill will be handled in the UpdateManaSpent method
            IncreaseSkillCounter(SkillInUse, 1);

        //the player cannot attack if he does not have enough mana to use the magic weapon
        if (combatParameter.UsingWeapon && Inventory.Weapon is MagicWeapon magicWeapon &&
            !Group.FlagIsEnabled(PlayerFlag.HasInfiniteMana))
        {
            DecreaseMana(magicWeapon.ManaConsumption);
            UpdateManaSpent(magicWeapon.ManaConsumption);
        }
    }

    public override Result CanAttack(CombatParameter combatParameter)
    {
        var result = base.CanAttack(combatParameter);
        if (result.Failed) return result;

        if (Group.FlagIsEnabled(PlayerFlag.CannotAttackMonster) && Group.FlagIsEnabled(PlayerFlag.CannotAttackPlayer))
        {
            StopAttack();
            return Result.NotPossible;
        }

        var hasEnoughAmmo = Inventory.Weapon is INeedsAmmo distanceWeapon &&
                            distanceWeapon.CanShootAmmunition(Inventory.Ammo);

        if (combatParameter.UsingWeapon && Inventory.Weapon is INeedsAmmo && !hasEnoughAmmo) return Result.NotPossible;

        //the player cannot attack if he does not have enough mana to use the magic weapon
        if (combatParameter.UsingWeapon && Inventory.Weapon is MagicWeapon magicWeapon &&
            !HasEnoughMana(magicWeapon.ManaConsumption))
            return new Result(InvalidOperation.NotEnoughMana);

        return result;
    }

    public void StopAllActions()
    {
        StopWalking();
        StopAttack();
        StopFollowing();
    }

    public bool CanUseOutfit(Outfit.Outfit outfit)
    {
        if (string.IsNullOrEmpty(outfit.Name)) return false;
        if (outfit.Premium && !HasPremiumTime) return false;

        return outfit.Unlocked;
    }

    public override void ChangeOutfit(Outfit.Outfit outfit)
    {
        if (!CanUseOutfit(outfit)) return;
        if (IsInvisible) return;

        base.ChangeOutfit(outfit);
    }

    public void IncreaseSkillCounter(SkillType skill, long value)
    {
        if (!Skills.ContainsKey(skill)) return;

        var rate = Creatures.Player.Vocation.Vocation.DefaultSkillMultiplier;

        Vocation?.Skills?.TryGetValue(skill, out rate);

        Skills[skill].IncreaseCounter(value, rate);
    }

    public void DecreaseSkillCounter(SkillType skill, long value)
    {
        if (!Skills.ContainsKey(skill)) return;

        var rate = Creatures.Player.Vocation.Vocation.DefaultSkillMultiplier;

        Vocation?.Skills?.TryGetValue(skill, out rate);

        Skills[skill].DecreaseCounter(value, rate);
    }

    public virtual void RemoveLogoutBlock()
    {
        RemoveCondition(ConditionType.LogoutBlock);
    }

    public bool IsLogoutBlocked => HasCondition(ConditionType.LogoutBlock);

    public override void Kill(ICombatActor enemy, bool lastHit = false, bool unjustified = false)
    {
        if (enemy is IPlayer playerEnemy && playerEnemy.GetSkull(this) is Skull.None)
        {
            NumberOfUnjustifiedKillsLastDay++;
            NumberOfUnjustifiedKillsLastWeek++;
            NumberOfUnjustifiedKillsLastMonth++;
            unjustified = true;
        }

        base.Kill(enemy, lastHit, unjustified);
    }

    public Skull GetSkull(IPlayer enemy)
    {
        return PlayerSkull?.GetSkull(enemy) ?? Skull.None;
    }

    public void SetSkull(Skull skull, DateTime? skullEndingDate = null, IPlayer enemy = null)
    {
        PlayerSkull?.SetSkull(skull, skullEndingDate, enemy);
    }

    public void RemoveSkull()
    {
        PlayerSkull?.RemoveSkull();
    }

    public void SetNumberOfKills(int killsInLastDay, int killsInLastWeek, int killsInLastMonth)
    {
        NumberOfUnjustifiedKillsLastDay = killsInLastDay;
        NumberOfUnjustifiedKillsLastWeek = killsInLastWeek;
        NumberOfUnjustifiedKillsLastMonth = killsInLastMonth;
    }

    public override CombatDamage ReduceDamage(CombatDamage attack)
    {
        Inventory.Protect(attack);
        return base.ReduceDamage(attack);
    }

    public Result CanCastSpell(ISpell spell)
    {
        if (Group.FlagIsEnabled(PlayerFlag.CannotUseSpells)) return Result.Fail(InvalidOperation.CannotUseSpells);

        if (Group.FlagIsEnabled(PlayerFlag.IgnoreSpellCheck)) return Result.Success;

        if (!spell.VocationIds?.Contains(((IPlayer)this).VocationType) ?? false)
            return Result.Fail(InvalidOperation.VocationCannotUseSpell);

        if (spell.NeedsPremium && !HasPremiumTime) return Result.Fail(InvalidOperation.PremiumTimeIsRequired);

        if (spell.IsAggressive && (spell.Range < 1 || (spell.Range > 0 && CurrentTarget is null)) &&
            Skull is Skull.Black)
            return Result.NotPossible;

        if (!HasEnoughLevel(spell.MinLevel)) return Result.Fail(InvalidOperation.NotEnoughLevel);

        if (MagicLevel < spell.MinMagicLevel) return Result.Fail(InvalidOperation.NotEnoughLevel);

        if (spell.IsAggressive && IsPacified) return Result.Fail(InvalidOperation.NotPermittedInProtectionZone);

        if (spell.IsAggressive && !Group.FlagIsEnabled(PlayerFlag.IgnoreProtectionZone) && Tile.ProtectionZone)
            return Result.Fail(InvalidOperation.NotPermittedInProtectionZone);

        if (spell.NeedWeapon && !Inventory.IsUsingWeapon) return Result.Fail(InvalidOperation.SpellNeedsWeapon);

        if (!HasEnoughMana(spell.ManaConsumption) && !Group.FlagIsEnabled(PlayerFlag.HasInfiniteMana))
            return Result.Fail(InvalidOperation.NotEnoughMana);

        if (!HasEnoughSoul(spell.SoulConsumption) && !Group.FlagIsEnabled(PlayerFlag.HasInfiniteSoul))
            return Result.Fail(InvalidOperation.NotEnoughSoul);

        if (spell.NeedLearn)
            //todo: implement learn validation
            throw new NotImplementedException();


        if (!CooldownHasExpired(spell)) return Result.Fail(InvalidOperation.Exhausted);

        return Result.Success;
    }

    public Result CanPushCreature(ICreature creature, ITile destination)
    {
        // Basic null checks
        if (creature is null || destination is null) return Result.Fail(InvalidOperation.NotPossible);


        // Cannot push yourself
        if (ReferenceEquals(creature, this)) return Result.Fail(InvalidOperation.DestinationOutOfReach);

        // Check if the player can push all creatures
        if (Group.FlagIsEnabled(PlayerFlag.CanPushAllCreatures)) return Result.Success;

        // Check cooldown (only for non-admin players)
        if (!CooldownHasExpired(CooldownType.PushCreature) && !Group.Access)
            return Result.Fail(InvalidOperation.Exhausted);

        // Check if the player can see the target creature
        if (!CanSee(creature)) return Result.NotPossible;

        // Check if the target is close enough to push
        if (!creature.IsCloseTo(this)) return Result.NotPossible;

        // Check if the destination is within 1 tile of the target
        var distance = creature.Location.GetMaxSqmDistance(destination.Location);
        if (distance > 1) return Result.Fail(InvalidOperation.DestinationOutOfReach);

        // Cannot push to the same location where creature currently is
        if (creature.Location == destination.Location) return Result.Success; // Not an error, just no movement needed

        // Check if the destination tile has another creature
        if (destination is IDynamicTile { HasAnyCreature: true }) return Result.Fail(InvalidOperation.NotEnoughRoom);

        // Check if destination tile blocks path
        if (destination is IDynamicTile destinationTile && destinationTile.HasFlag(TileFlags.BlockPath))
            return Result.NotPossible;

        // Check push permissions based on a creature type
        switch (creature)
        {
            case IPlayer targetPlayer:
            {
                // Check if the target player has CannotBePushed flag (with null safety)
                if (targetPlayer.Group?.FlagIsEnabled(PlayerFlag.CannotBePushed) == true)
                    return Result.Fail(InvalidOperation.NotPossible);

                // Cannot push players out of the protection zone
                var pushingOutsideProtectionZone = destination is IDynamicTile { ProtectionZone: false } &&
                                                   (targetPlayer.Tile?.ProtectionZone ?? false);
                if (pushingOutsideProtectionZone) return Result.NotPossible;
                break;
            }

            case IMonster targetMonster:
            {
                // Check if monster is pushable
                if (!targetMonster.IsPushable) return Result.NotPossible;

                // Cannot push monsters into protection zone
                var pushingToProtectionZone = destination is IDynamicTile { ProtectionZone: true };
                if (pushingToProtectionZone) return Result.NotPossible;
                break;
            }

            case INpc:
            {
                // Cannot push NPCs into protection zone
                var pushingToProtectionZone = destination is IDynamicTile { ProtectionZone: true };
                if (pushingToProtectionZone) return Result.NotPossible;
                break;
            }
        }

        return Result.Success;
    }

    public override void AddCondition(ICondition condition)
    {
        if (Group.FlagIsEnabled(PlayerFlag.CannotBeAttacked) && condition.Type.ToDamageType() != DamageType.None)
            return;

        switch (condition.Type)
        {
            case ConditionType.Drunk when Inventory.HasEquippedItemWithImmunity(Immunity.Drunkenness):
            case ConditionType.Drowning when Inventory.HasEquippedItemWithImmunity(Immunity.Drown):
                return;
            default:
                base.AddCondition(condition);
                break;
        }
    }

    public void MoveToTemple()
    {
        SetNewLocation(new Location(Town.Coordinate));
    }

    public override DamageResult TakeDamage(IThing enemy, CombatDamageList damages)
    {
        if (Group.FlagIsEnabled(PlayerFlag.CannotBeAttacked)) return new DamageResult(new CombatDamageList(), false);

        return base.TakeDamage(enemy, damages);
    }

    public void HealSoul(ushort increasing)
    {
        if (increasing <= 0) return;

        if (SoulPoints == MaxSoulPoints) return;

        SoulPoints = SoulPoints + increasing >= MaxSoulPoints ? MaxSoulPoints : (byte)(SoulPoints + increasing);
        OnStatusChanged?.Invoke(this);
    }

    public long ApplyStaminaEffectOnExperienceGain(long experience)
    {
        if (HasNoStamina) return 0;

        if (HasLowStamina)
        {
            // Experience gain is halved when stamina is below threshold
            experience += experience * GameConstants.STAMINA_THRESHOLD_EXP_PERCENTAGE / 100;
            return experience;
        }

        if (HasStaminaBonus && HasPremiumTime)
            experience += experience * GameConstants.STAMINA_BONUS_EXP_PERCENTAGE / 100;

        return experience;
    }

    public void ConsumeStamina(ushort seconds = 60)
    {
        StaminaMinutes = (ushort)Math.Max(StaminaMinutes - seconds / TimeSpan.SecondsPerMinute, 0);
    }

    public void RecoverStamina(uint staminaMinutes)
    {
        StaminaMinutes = (ushort)Math.Min(StaminaMinutes + staminaMinutes, GameConstants.STAMINA_MAX_MINUTES);
    }

    public void HandleTargetLost()
    {
        if (!IsTargetLost()) return;

        var showError = CurrentTarget is not ICombatActor { IsDead: true };

        StopAttack();

        if (showError) OperationFailService.Send(this, InvalidOperation.TargetLost);
    }


    private ushort CalculateTotalAttack(ushort attackPower, bool isElemental = false)
    {
        var damageMultiplier = SkillInUse switch
        {
            SkillType.Distance => Vocation.Formula?.DistDamage ?? 1f,
            SkillType.Magic => 1f,
            _ => Vocation.Formula?.MeleeDamage ?? 1f
        };

        var attackPercentage = 100;

        if (Inventory.Weapon is IHasAttack weapon)
            attackPercentage = isElemental
                ? weapon.WeaponAttack.ElementalAttackPowerPercentage
                : weapon.WeaponAttack.AttackPowerPercentage;

        if (Inventory.Weapon is MagicWeapon magicalWeapon) return magicalWeapon.MaxHitChance;

        if (Inventory.Weapon is IDistanceWeapon && Inventory.Ammo is { } ammo)
            attackPercentage = isElemental
                ? ammo.WeaponAttack.ElementalAttackPowerPercentage
                : ammo.WeaponAttack.AttackPowerPercentage;

        var maximumAttack = (ushort)(Inventory.AttackRate * DamageFactor * attackPower * Skills[SkillInUse].Level +
                                     Level / 5 * damageMultiplier);

        return (ushort)(maximumAttack * attackPercentage / 100);
    }

    public override CalculatedAttackDamage CalculateAttackDamage()
    {
        return base.CalculateAttackDamage();
    }

    private Result CanUseItem(IUsableOn item, Location onLocation)
    {
        if (!Cooldowns.Expired(CooldownType.UseItem))
        {
            OnExhausted?.Invoke(this);
            {
                return Result.Fail(InvalidOperation.Exhausted);
            }
        }

        if (MapTool.SightClearChecker?.Invoke(Location, onLocation, true) == false)
        {
            OperationFailService.Send(CreatureId, TextConstants.CANNOT_THROW_THERE);
            {
                return Result.Fail(InvalidOperation.CannotThrowThere);
            }
        }

        if (!item.IsCloseTo(this)) return Result.Fail(InvalidOperation.TooFar);

        if (item is IEquipmentRequirement requirement && !requirement.CanBeUsed(this))
        {
            OperationFailService.Send(CreatureId, requirement.ValidationError);
            {
                return Result.Fail(InvalidOperation.CannotUseSpells);
            }
        }

        return Result.Success;
    }

    public void RemoveHungry()
    {
        RemoveCondition(ConditionType.Hungry);
    }

    public void ResetIdleTime()
    {
        IdleTime = 0;
    }

    public bool CanMoveThing(Location location)
    {
        return Location.GetSqmDistance(location) <= MapConstants.MAX_DISTANCE_MOVE_THING;
    }

    public void OnLevelAdvance(SkillType type, int fromLevel, int toLevel)
    {
        if (type == SkillType.Level)
        {
            var levelDiff = toLevel - fromLevel;
            MaxHealthPoints += (uint)(levelDiff * Vocation.GainHp);
            MaxMana += (ushort)(levelDiff * Vocation.GainMana);
            if (!Group.FlagIsEnabled(PlayerFlag.HasInfiniteCapacity))
                TotalCapacity += (uint)(levelDiff * Vocation.GainCap);
            ResetHealthPoints();
            ResetMana();
            ChangeSpeedLevel(RawSpeed);
        }

        OnLevelAdvanced?.Invoke(this, type, fromLevel, toLevel);
    }

    private void OnLevelRegress(SkillType type, int fromLevel, int toLevel)
    {
        if (type == SkillType.Level)
        {
            var levelDiff = toLevel - fromLevel;
            MaxHealthPoints += (uint)(levelDiff * Vocation.GainHp);
            MaxMana += (ushort)(levelDiff * Vocation.GainMana);
            if (!Group.FlagIsEnabled(PlayerFlag.HasInfiniteCapacity))
                TotalCapacity += (uint)(levelDiff * Vocation.GainCap);
            ResetHealthPoints();
            ResetMana();
            ChangeSpeedLevel(RawSpeed);
        }

        OnLevelRegressed?.Invoke(this, type, fromLevel, toLevel);
    }

    public void ResetMana()
    {
        IncreaseMana(MaxMana);
    }

    public override bool IsImmune(Immunity immunity)
    {
        return false;
        //todo: add immunity check
    }

    public virtual void SetLogoutBlock()
    {
        if (Group.FlagIsEnabled(PlayerFlag.NotGainInFight)) return;

        if (IsPacified) return;

        if (HasCondition(ConditionType.LogoutBlock, out var condition))
        {
            condition.Start(this);
            return;
        }

        if (IsProtectionZoneBlocked)
            //resets protection zone block time
            SetProtectionZoneBlock();

        //logout is persistent, this will be removed elsewhere
        AddCondition(new Condition(ConditionType.LogoutBlock, 0));
    }

    private void TogglePacifiedCondition(IDynamicTile fromTile, IDynamicTile toTile)
    {
        if (toTile is null) return;
        switch (fromTile?.ProtectionZone)
        {
            case null when toTile.ProtectionZone:
                AddCondition(new Condition(ConditionType.Pacified, 0));
                RemoveProtectionZoneBlock();
                break;
            case false when toTile.ProtectionZone:
                RemoveLogoutBlock();
                RemoveProtectionZoneBlock();
                AddCondition(new Condition(ConditionType.Pacified, 0));
                break;
            case true when toTile.ProtectionZone is false:
                RemoveCondition(ConditionType.Pacified);
                break;
        }
    }

    public override bool TryWalkTo(params Direction[] directions)
    {
        if (directions is null or { Length: 0 }) return false;

        if (HasCondition(ConditionType.Drunk))
        {
            // Only allow North, East, South, West (no diagonals)
            Direction[] nonDiagonalDirections = [Direction.North, Direction.East, Direction.South, Direction.West];

            for (var i = 0; i < directions.Length; i++)
                // Replace the direction every 2 steps
                if (i % 3 == 0)
                {
                    var oldDirection = directions[i];
                    var newDirection =
                        nonDiagonalDirections[GameRandom.Random.Next(maxValue: nonDiagonalDirections.Length)];
                    if (oldDirection == newDirection) continue;

                    directions[i] =
                        nonDiagonalDirections[GameRandom.Random.Next(maxValue: nonDiagonalDirections.Length)]
                            .MakeDrunk();
                }
        }

        ResetIdleTime();
        return base.TryWalkTo(directions);
    }

    public override CombatDamage OnImmunityDefense(CombatDamage damage)
    {
        if (!IsImmune(damage.Type.ToImmunity())) return damage;
        damage.SetNewDamage(0);
        return damage;
    }

    public void ChangeOnlineStatus(bool online)
    {
        Online = online;
        OnChangedOnlineStatus?.Invoke(this, online);
    }

    public override bool CanBlock(DamageType damage)
    {
        return Inventory.HasShield && base.CanBlock(damage);
    }

    public override void OnDamage(IThing enemy, CombatDamageList damages)
    {
        SetLogoutBlock();

        var totalDamage = damages.TotalDamage;

        if (totalDamage.ManaDamage > 0) DecreaseMana(totalDamage.ManaDamage);

        if (IsManaShieldEnabled && Mana > 0)
        {
            var totalHealthDamage = Math.Min(totalDamage.HealthDamage, (int)Mana);
            damages.ReduceHealthDamage(totalHealthDamage);

            DecreaseMana((uint)totalHealthDamage);
            damages.AddDamage(new CombatDamage((ushort)totalHealthDamage, DamageType.ManaDrain));

            totalDamage = damages.TotalDamage;
        }

        ReduceHealth(totalDamage.HealthDamage);
    }

    public override void Death(IThing by)
    {
        base.Death(by);

        PlayerSkull.RemoveYellowSkull();
        DecreaseExp();
    }

    private void DecreaseExp()
    {
        var lostExperience = CalculateLostExperience();
        Skills.TryGetValue(SkillType.Level, out var value);
        value.DecreaseLevel(lostExperience);
    }

    private double CalculateLostExperience()
    {
        if (Level <= 23) return 10 * 0.01 * Experience;

        var expLost = (Level + 50) / 100.0 * 50 * (Math.Pow(Level, 2) - 5 * Level + 8);

        if (IsPromoted) expLost -= expLost * .30;

        return expLost;
    }

    #region Stamina

    public ushort StaminaMinutes { get; private set; }
    public bool HasLowStamina => StaminaMinutes <= GameConstants.STAMINA_THRESHOLD_MINUTES;
    public bool HasStaminaBonus => StaminaMinutes >= GameConstants.STAMINA_BONUS_MINUTES;
    public bool HasNoStamina => StaminaMinutes <= 0;
    public bool IgnoreStamina => Group.FlagIsEnabled(PlayerFlag.IgnoreStamina);

    #endregion

    #region Storage

    //TODO: rename this method to something more meaningful or take this from here if this is not game business rule
    public IDictionary<uint, int> Storages { get; }

    public int GetStorageValue(uint key)
    {
        return Storages.TryGetValue(key, out var storage) ? storage : -1;
    }

    public void AddOrUpdateStorageValue(uint key, int value)
    {
        var oldValue = GetStorageValue(key);
        Storages.AddOrUpdate(key, value);
        //todo: implement current time
        EventAggregator.Invoke(new PlayerStorageUpdateEvent(this, key, value, oldValue, 0));
    }

    public override void Think(int interval)
    {
        HandleTargetLost();
        EventAggregator.Invoke(new PlayerThinkEvent(this, interval));
    }

    public override bool IsTargetLost()
    {
        if (CurrentTarget is IPlayer && (CurrentTarget.Tile?.NoPvpZone ?? false)) return true;
        return base.IsTargetLost();
    }

    #endregion

    #region Guild

    public ushort? GuildId { get; set; }
    public ushort GuildLevel { get; set; }
    public string GuildNick { get; set; } = string.Empty;
    public bool HasGuild => Guild is not null;
    public Guild.Guild Guild { get; set; }
    public GuildRankInfo GuildRank { get; set; }
    public List<ushort> GuildWarList { get; } = new();

    public bool IsGuildMate(IPlayer otherPlayer)
    {
        if (!HasGuild || otherPlayer?.Guild == null) return false;
        return Guild?.Id == otherPlayer.Guild?.Id;
    }

    public bool IsInWar(IPlayer otherPlayer)
    {
        if (!HasGuild || otherPlayer?.Guild == null) return false;
        return Guild?.IsInWar(otherPlayer) == true;
    }

    public bool IsInWarList(ushort guildId)
    {
        return GuildWarList.Contains(guildId);
    }

    public void SetGuild(Guild.Guild guild)
    {
        if (Guild == guild) return;

        var oldGuild = Guild;

        // Clear guild data
        GuildNick = string.Empty;
        Guild = null;
        GuildRank = null;

        if (guild != null)
        {
            // Set new guild
            Guild = guild;
            GuildId = guild.Id;

            // Get default rank (level 1)
            var defaultRank = guild.GetRankByLevel(1);
            if (defaultRank != null)
            {
                GuildRank = defaultRank;
                guild.AddMember(this);
            }
        }
        else
        {
            GuildId = null;
        }

        // Remove from old guild if switching
        if (oldGuild != null && oldGuild != guild) oldGuild.RemoveMember(this);
    }

    #endregion

    #region Equip/DeEquip

    public void OnDressedItem(IItem item)
    {
        OnEquipItem?.Invoke(this, item, true);
    }

    public void OnUndressedItem(IItem item)
    {
        OnDeEquipItem?.Invoke(this, item, true);
    }

    #endregion

    #region Events

    public event PlayerLevelAdvance OnLevelAdvanced;
    public event PlayerLevelRegress OnLevelRegressed;
    public event PlayerGainSkillPoint OnGainedSkillPoint;
    public event ReduceMana OnStatusChanged;
    public event LookAt OnLookedAt;
    public event UseItem OnUsedItem;
    public event ChangeOnlineStatus OnChangedOnlineStatus;
    public event SendMessageTo OnSentMessage;

    public event Exhaust OnExhausted;
    public event Hear OnHear;
    public event ChangeChaseMode OnChangedChaseMode;
    public event AddSkillBonus OnAddedSkillBonus;
    public event RemoveSkillBonus OnRemovedSkillBonus;
    public event WroteText OnWroteText;
    public event EquipItem OnEquipItem;
    public event DeEquipItem OnDeEquipItem;

    #endregion
}