using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Creatures.Players;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Common.Texts;
using NeoServer.Game.Creatures.Models;
using NeoServer.Game.Creatures.Models.Bases;
using static System.Net.Mime.MediaTypeNames;

namespace NeoServer.Game.Creatures.Player;

public class Player : CombatActor, IPlayer
{
    private const int KNOWN_CREATURE_LIMIT = 250; //todo: for version 8.60

    private uint _idleTime;
    private byte _soulPoints;

    public Player(
        uint id,
        string characterName,
        ChaseMode chaseMode,
        uint capacity,
        uint healthPoints,
        uint maxHealthPoints,
        IVocation vocation,
        IGroup group,
        Gender gender,
        bool online,
        ushort mana,
        ushort maxMana,
        FightMode fightMode,
        byte soulPoints,
        byte soulMax,
        IDictionary<SkillType, ISkill> skills,
        IDictionary<int, int> storages,
        ushort staminaMinutes,
        IOutfit outfit,
        ushort speed,
        Location location,
        IMapTool mapTool, ITown town)
        : base(
            new CreatureType(
                characterName,
                string.Empty,
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
        TotalCapacity = capacity;
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
        Speed = speed == 0 ? LevelBasesSpeed : speed;
        Inventory = new Inventory.Inventory(this, new Dictionary<Slot, (IItem Item, ushort Id)>());

        Vip = new Vip(this);
        Channels = new PlayerChannel(this);
        PlayerParty = new PlayerParty(this);
        PlayerHand = new PlayerHand(this);

        SetNewLocation(location);
        Town = town;

        Containers = new PlayerContainerList(this);

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

    private ushort LevelBasesSpeed => (ushort)(220 + 2 * (Level - 1));
    public string CharacterName { get; }
    public Dictionary<uint, long> KnownCreatures { get; }
    public bool Online { get; }

    public float DamageFactor => FightMode switch
    {
        FightMode.Attack => 1,
        FightMode.Balanced => 0.75f,
        FightMode.Defense => 0.5f,
        _ => 0.75f
    };

    public int DefenseFactor => FightMode switch
    {
        FightMode.Attack => 5,
        FightMode.Balanced => 7,
        FightMode.Defense => 10,
        _ => 7
    };

    public bool IsPacified => Conditions.ContainsKey(ConditionType.Pacified);
    public IPlayerHand PlayerHand { get; }

    public IDictionary<SkillType, ISkill> Skills { get; }

    /// <summary>
    ///     Gender pronoun: He/She
    /// </summary>
    public string GenderPronoun => Gender == Gender.Male ? "He" : "She";

    public Gender Gender { get; }
    public int PremiumTime { get; init; }
    public ITown Town { get; set; }
    public IVip Vip { get; }
    public override IOutfit Outfit { get; protected set; }
    public IVocation Vocation { get; }
    public IGroup Group { get; set; }
    public IPlayerChannel Channels { get; set; }
    public IPlayerParty PlayerParty { get; set; }
    public ulong BankAmount { get; private set; }

    public List<RegenerationBonus> RegenerationBonusList { get; private set; }

    public ulong GetTotalMoney(ICoinTypeStore coinTypeStore)
    {
        return BankAmount + Inventory.GetTotalMoney(coinTypeStore);
    }

    public void LoadBank(ulong amount)
    {
        BankAmount = amount;
    }

    public uint AccountId { get; init; }
    public int WorldId { get; init; }
    public IPlayerContainerList Containers { get; }
    public bool HasDepotOpened => Containers.HasAnyDepotOpened;
    public IShopperNpc TradingWithNpc { get; private set; }
    public ChaseMode ChaseMode { get; private set; }
    public uint TotalCapacity { get; private set; }
    public ushort Level => (ushort)(Skills.TryGetValue(SkillType.Level, out var level) ? level?.Level ?? 1 : 1);
    public ushort Mana { get; private set; }
    public ushort MaxMana { get; private set; }
    public FightMode FightMode { get; private set; }

    public bool Shopping => TradingWithNpc is not null;

    public byte SoulPoints
    {
        get => _soulPoints;
        private set => _soulPoints = value > MaxSoulPoints ? MaxSoulPoints : value;
    }

    public byte MaxSoulPoints { get; }

    public IInventory Inventory { get; private set; }
    public ushort StaminaMinutes { get; }

    public uint Experience
    {
        get
        {
            if (Skills.TryGetValue(SkillType.Level, out var skill)) return (uint)skill.Count;
            return 0;
        }
    }

    public void AddInventory(IInventory inventory)
    {
        Inventory = inventory;
    }

    public byte LevelPercent => GetSkillPercent(SkillType.Level);

    public override void GainExperience(long exp)
    {
        if (exp == 0) return;

        IncreaseSkillCounter(SkillType.Level, exp);
        base.GainExperience(exp);
    }

    public override void LoseExperience(long exp)
    {
        if (exp == 0) return;

        DecreaseSkillCounter(SkillType.Level, exp);
        base.LoseExperience(exp);
    }

    public override decimal AttackSpeed => Vocation.AttackSpeed == default ? base.AttackSpeed : Vocation.AttackSpeed;

    public virtual bool CannotLogout => !(Tile?.ProtectionZone ?? false) && InFight;

    public virtual bool IsProtectionZoneLocked => false;

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
    public override ushort MinimumAttackPower => (ushort)(Level / 5);
    public override ushort ArmorRating => Inventory.TotalArmor;
    public byte SecureMode { get; private set; }
    public float FreeCapacity => TotalCapacity - Inventory.TotalWeight;
    public override bool UsingDistanceWeapon => Inventory.Weapon is IDistanceWeapon;
    public bool Recovering => HasCondition(ConditionType.Regeneration);
    public override bool CanSeeInvisible => Group.FlagIsEnabled(PlayerFlag.CanSenseInvisibility);
    public override bool CanBeSeen => Group.FlagIsEnabled(PlayerFlag.IgnoreYellCheck);
    public virtual bool CanSeeInspectionDetails => false;

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
        var rate = Creatures.Vocation.Vocation.DefaultSkillMultiplier;
        Vocation.Skills?.TryGetValue(skill, out rate);
        return (byte)Skills[skill].GetPercentage(rate);
    }

    public bool KnowsCreatureWithId(uint creatureId)
    {
        return KnownCreatures.ContainsKey(creatureId);
    }

    public void AddKnownCreature(uint creatureId)
    {
        KnownCreatures.TryAdd(creatureId, DateTime.Now.Ticks);
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
        TogglePacifiedCondition(fromTile, toTile);
        Containers.CloseDistantContainers();
        base.OnMoved(fromTile, toTile, spectators);
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
        SetAsInFight();
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

    public void ChangeSecureMode(byte mode)
    {
        SecureMode = mode;
    }

    public override int DefendUsingShield(int attack)
    {
        if (!IsShieldDefenseEnabled) return attack;

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

    public virtual bool CastSpell(string message)
    {
        if (!SpellList.TryGet(message.Trim(), out var spell)) return false;
        if (!spell.Invoke(this, message, out var error))
        {
            OnCannotUseSpell?.Invoke(this, spell, error);
            return true;
        }

        var talkType = SpeechType.MonsterSay;

        Cooldowns.Start(CooldownType.Spell, 1000); //todo: 1000 should be a const

        if (spell.IncreaseSkill) IncreaseSkillCounter(SkillType.Magic, spell.Mana);

        if (!spell.ShouldSay) return true;

        base.Say(message, talkType);

        return true;
    }

    public bool HasEnoughMana(ushort mana)
    {
        return Mana >= mana;
    }

    public void ConsumeMana(ushort mana)
    {
        if (mana == 0) return;
        if (!HasEnoughMana(mana)) return;

        Mana -= mana;
        OnStatusChanged?.Invoke(this);
    }

    public bool HasEnoughLevel(ushort level)
    {
        return Level >= level;
    }

    public void LookAt(ITile tile)
    {
        var isClose = Location.IsNextTo(tile.Location);
        if (tile.TopCreatureOnStack is null && tile.TopItemOnStack is null) return;

        IThing thing = tile.TopCreatureOnStack is null ? tile.TopItemOnStack : tile.TopCreatureOnStack;
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
        OnReadText?.Invoke(this, readable, readable.Text);
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
        if (CannotLogout && forced == false)
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

        OnLoggedOut?.Invoke(this);
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
        OnLoggedIn?.Invoke(this);

        return true;
    }

    public void HealMana(ushort increasing)
    {
        if (increasing <= 0) return;

        if (Mana == MaxMana) return;

        Mana = Mana + increasing >= MaxMana ? MaxMana : (ushort)(Mana + increasing);
        OnStatusChanged?.Invoke(this);
    }

    public void Recover()
    {
        if (!Recovering) return;

        if (Cooldowns.Expired(CooldownType.HealthRecovery)) Heal(Vocation.GainHpAmount, this);
        if (Cooldowns.Expired(CooldownType.ManaRecovery)) HealMana(Vocation.GainManaAmount);
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
                    HealMana(regenerationBonus.Gain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //todo: start these cooldowns when player logs in
        Cooldowns.Start(CooldownType.HealthRecovery, Vocation.GainHpTicks * 1000);
        Cooldowns.Start(CooldownType.ManaRecovery, Vocation.GainManaTicks * 1000);
        Cooldowns.Start(CooldownType.SoulRecovery, Vocation.GainSoulTicks * 1000);

        foreach (var regenerationBonus in RegenerationBonusList)
        {
            Cooldowns.Start(regenerationBonus.Id, regenerationBonus.Ticks);
        }
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
                case IUsableAttackOnCreature usableAttackOnCreature:
                    itemUsed = Attack(enemy, usableAttackOnCreature);
                    break;
                case IUsableOnCreature usableOnCreature:
                    usableOnCreature.Use(this, onCreature);
                    itemUsed = true;
                    break;
                case IUsableOnTile useableOnTile:
                    itemUsed = useableOnTile.Use(this, onCreature.Tile);
                    break;
                case IUsableOnItem useableOnItem:
                    itemUsed = useableOnItem.Use(this, onCreature.Tile.TopItemOnStack);
                    break;
            }

        if (itemUsed)
        {
            OnUsedItem?.Invoke(this, onCreature, item);
            Cooldowns.Start(CooldownType.UseItem, item.CooldownTime);
            return Result.Success;
        }

        OperationFailService.Send(CreatureId, TextConstants.NOT_POSSIBLE);
        return Result.Fail(InvalidOperation.NotPossible);
    }

    public Result Use(IUsableOn item, IItem onItem)
    {
        var canUseItem = CanUseItem(item, onItem.Location);
        if (canUseItem.Failed) return canUseItem;

        if (item is not IUsableOnItem usableOnItem) return Result.Fail(InvalidOperation.CannotUse);

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

        if (targetTile.TopItemOnStack is not { } onItem) return Result.NotPossible;

        var result = item switch
        {
            IUsableAttackOnTile usableAttackOnTile => Attack(targetTile, usableAttackOnTile),
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

    public bool Feed(IFood food)
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

    public void EnableManaShield(uint duration) =>
        AddCondition(new Condition(ConditionType.ManaShield, duration,
            () => { RemoveCondition(ConditionType.ManaShield); }));

    public void EnableManaShield() => AddCondition(new Condition(ConditionType.ManaShield));

    public void DisableManaShield() => RemoveCondition(ConditionType.ManaShield);

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

    public bool Sell(IItemType item, byte amount, bool ignoreEquipped)
    {
        if (!ignoreEquipped) return true;
        if (Inventory.BackpackSlot?.Map is null) return false;
        if (!Inventory.BackpackSlot.Map.TryGetValue(item.ServerId, out var itemTotalAmount)) return false;

        if (itemTotalAmount < amount) return false;

        Inventory.BackpackSlot.RemoveItem(item, amount);

        TradingWithNpc.BuyFromCustomer(this, item, amount);

        return true;
    }

    public void ReceivePayment(IEnumerable<IItem> coins, ulong total)
    {
        if (CanReceiveInCashPayment(coins))
            foreach (var coin in coins)
                Inventory.BackpackSlot.AddItem(coin, true);
        else
            BankAmount += total;
    }

    public virtual void WithdrawFromBank(ulong amount)
    {
        if (BankAmount >= amount) BankAmount -= amount;
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
                    if (!result.Value.HasAnyOperation) continue;
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

    public override Result OnAttack(ICombatActor enemy, out CombatAttackResult[] combatAttacks)
    {
        combatAttacks = new CombatAttackResult[1];

        var canUse = true;

        var combat = CombatAttackResult.None;

        if (Inventory.IsUsingWeapon) canUse = Inventory.Weapon.Attack(this, enemy, out combat);

        if (!Inventory.IsUsingWeapon) FistCombatAttack.Use(this, enemy, out combat);

        if (canUse) IncreaseSkillCounter(SkillInUse, 1);

        combatAttacks[0] = combat;

        SetAsInFight();

        return canUse ? Result.Success : Result.Fail(InvalidOperation.CannotUseWeapon);
    }

    public override Result Attack(ICombatActor enemy)
    {
        if (enemy.IsInvisible)
        {
            StopAttack();
            return Result.Fail(InvalidOperation.AttackTargetIsInvisible);
        }

        return base.Attack(enemy);
    }

    public void StopAllActions()
    {
        StopWalking();
        StopAttack();
        StopFollowing();
    }

    public bool CanUseOutfit(IOutfit outfit)
    {
        if (string.IsNullOrEmpty(outfit.Name)) return false;
        if (outfit.Premium && !(PremiumTime > 0)) return false;

        return outfit.Unlocked;
    }

    public override void ChangeOutfit(IOutfit outfit)
    {
        if (!CanUseOutfit(outfit)) return;
        if (IsInvisible) return;

        base.ChangeOutfit(outfit);
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
                return Result.Fail(InvalidOperation.CannotUse);
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
        _idleTime = 0;
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
            TotalCapacity += (uint)(levelDiff * Vocation.GainCap);
            ResetHealthPoints();
            ResetMana();
            ChangeSpeedLevel(LevelBasesSpeed);
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
            TotalCapacity += (uint)(levelDiff * Vocation.GainCap);
            ResetHealthPoints();
            ResetMana();
            ChangeSpeedLevel(LevelBasesSpeed);
        }

        OnLevelRegressed?.Invoke(this, type, fromLevel, toLevel);
    }

    public void ResetMana()
    {
        HealMana(MaxMana);
    }

    public void IncreaseSkillCounter(SkillType skill, long value)
    {
        if (!Skills.ContainsKey(skill)) return;

        var rate = Creatures.Vocation.Vocation.DefaultSkillMultiplier;

        Vocation?.Skills?.TryGetValue(skill, out rate);

        Skills[skill].IncreaseCounter(value, rate);
    }

    public void DecreaseSkillCounter(SkillType skill, long value)
    {
        if (!Skills.ContainsKey(skill)) return;

        var rate = Creatures.Vocation.Vocation.DefaultSkillMultiplier;

        Vocation?.Skills?.TryGetValue(skill, out rate);

        Skills[skill].DecreaseCounter(value, rate);
    }

    public override bool HasImmunity(Immunity immunity)
    {
        return false;
        //todo: add immunity check
    }

    public virtual void SetAsInFight()
    {
        if (Group.FlagIsEnabled(PlayerFlag.NotGainInFight)) return;
        
        if (IsPacified) return;

        if (HasCondition(ConditionType.InFight, out var condition))
        {
            condition.Start(this);
            return;
        }

        AddCondition(new Condition(ConditionType.InFight, 60000));
    }

    private void TogglePacifiedCondition(IDynamicTile fromTile, IDynamicTile toTile)
    {
        switch (fromTile?.ProtectionZone)
        {
            case null when toTile.ProtectionZone:
                AddCondition(new Condition(ConditionType.Pacified, 0));
                break;
            case false when toTile.ProtectionZone:
                RemoveCondition(ConditionType.InFight);
                AddCondition(new Condition(ConditionType.Pacified, 0));
                break;
            case true when toTile.ProtectionZone is false:
                RemoveCondition(ConditionType.Pacified);
                break;
        }
    }

    public override bool TryWalkTo(params Direction[] directions)
    {
        ResetIdleTime();
        return base.TryWalkTo(directions);
    }

    public override CombatDamage OnImmunityDefense(CombatDamage damage)
    {
        if (!HasImmunity(damage.Type.ToImmunity())) return damage;
        damage.SetNewDamage(0);
        return damage;
    }

    public void ChangeOnlineStatus(bool online)
    {
        OnChangedOnlineStatus?.Invoke(this, online);
    }

    public override bool CanBlock(DamageType damage)
    {
        return Inventory.HasShield && base.CanBlock(damage);
    }

    public void HealSoul(ushort increasing)
    {
        if (increasing <= 0) return;

        if (SoulPoints == MaxSoulPoints) return;

        SoulPoints = SoulPoints + increasing >= MaxSoulPoints ? MaxSoulPoints : (byte)(SoulPoints + increasing);
        OnStatusChanged?.Invoke(this);
    }

    public override void OnDamage(IThing enemy, CombatDamage damage)
    {
        SetAsInFight();

        if (damage.Type is DamageType.ManaDrain || IsManaShieldEnabled)
        {
            ConsumeMana(damage.Damage);
            return;
        }

        ReduceHealth(damage);
    }

    public override ILoot DropLoot()
    {
        return null;
    }

    public override void Death(IThing by)
    {
        base.Death(by);
        DecreaseExp();
        MoveToTemple();
    }

    private void MoveToTemple()
    {
        SetNewLocation(new Location(Town.Coordinate));
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
        return (Level + 50) * .01 * 50 * (Math.Pow(Level, 2) - 5 * Level + 8);
    }

    #region Storage

    public IDictionary<int, int> Storages { get; }

    public int GetStorageValue(int key)
        => Storages.TryGetValue(key, out var storage) ? storage : -1;

    public void AddOrUpdateStorageValue(int key, int value)
        => Storages.AddOrUpdate(key, value);

    #endregion

    #region Guild

    public ushort GuildLevel { get; set; }
    public bool HasGuild => Guild is not null;
    public IGuild Guild { get; init; }

    #endregion

    #region Events

    public event PlayerLevelAdvance OnLevelAdvanced;
    public event PlayerLevelRegress OnLevelRegressed;
    public event PlayerGainSkillPoint OnGainedSkillPoint;
    public event ReduceMana OnStatusChanged;
    public event CannotUseSpell OnCannotUseSpell;
    public event LookAt OnLookedAt;
    public event UseSpell OnUsedSpell;
    public event UseItem OnUsedItem;
    public event LogIn OnLoggedIn;
    public event LogOut OnLoggedOut;
    public event ChangeOnlineStatus OnChangedOnlineStatus;
    public event SendMessageTo OnSentMessage;

    public event Exhaust OnExhausted;
    public event Hear OnHear;
    public event ChangeChaseMode OnChangedChaseMode;
    public event AddSkillBonus OnAddedSkillBonus;
    public event RemoveSkillBonus OnRemovedSkillBonus;
    public event ReadText OnReadText;
    public event WroteText OnWroteText;

    #endregion
}