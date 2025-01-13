using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures.Players;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void ChangeChaseMode(IPlayer player, ChaseMode oldChaseMode, ChaseMode newChaseMode);

public delegate void ClosedContainer(IPlayer player, byte containerId, IContainer container);

public delegate void ClosedDepot(IPlayer player, byte containerId, IDepot container);

public delegate void OpenedContainer(IPlayer player, byte containerId, IContainer container);

public delegate void ReduceMana(IPlayer player);

public delegate void CannotUseSpell(IPlayer player, ISpell spell, InvalidOperation error);

public delegate void PlayerLevelAdvance(IPlayer player, SkillType type, int fromLevel, int toLevel);

public delegate void PlayerLevelRegress(IPlayer player, SkillType type, int fromLevel, int toLevel);

public delegate void LookAt(IPlayer player, IThing thing, bool isClose);

public delegate void PlayerGainSkillPoint(IPlayer player, SkillType type);

public delegate void UseItem(IPlayer player, IThing thing, IUsableOn item);

public delegate void LogIn(IPlayer player);

public delegate void LogOut(IPlayer player);

public delegate void AddToVipList(IPlayer player, uint vipPlayerId, string vipPlayerName);

public delegate void PlayerLoadVipList(IPlayer player, IEnumerable<(uint, string)> vipList);

public delegate void ChangeOnlineStatus(IPlayer player, bool online);

public delegate void SendMessageTo(ISociableCreature from, ISociableCreature to, SpeechType speechType,
    string message);

public delegate void Exhaust(IPlayer player);

public delegate void AddSkillBonus(IPlayer player, SkillType skillType, sbyte increased);

public delegate void RemoveSkillBonus(IPlayer player, SkillType skillType, sbyte decreased);

public delegate void ReadText(IPlayer player, IReadable readable, string text);

public delegate void WroteText(IPlayer player, IReadable readable, string text);
public delegate void SkullUpdated(IPlayer player);

public interface IPlayer : ICombatActor, ISociableCreature
{
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

    ushort Level { get; }

    byte LevelPercent { get; }

    uint Experience { get; }
    byte SoulPoints { get; }

    float FreeCapacity { get; }

    ushort StaminaMinutes { get; }

    FightMode FightMode { get; }
    ChaseMode ChaseMode { get; }
    byte SecureMode { get; }

    new bool InFight { get; }
    IPlayerContainerList Containers { get; }

    ITown Town { get; }

    IInventory Inventory { get; }
    ushort Mana { get; }
    ushort MaxMana { get; }
    SkillType SkillInUse { get; }
    bool CannotLogout { get; }
    bool IsProtectionZoneLocked { get; }
    uint Id { get; }
    bool HasDepotOpened { get; }
    uint TotalCapacity { get; }
    bool Recovering { get; }
    IVocation Vocation { get; }
    byte VocationType => Vocation?.VocationType ?? default;
    IGroup Group { get; set; }
    byte GroupId => Group?.Id ?? default;
    uint AccountId { get; init; }
    int WorldId { get; init; }
    IGuild Guild { get; }
    ushort GuildId => Guild?.Id ?? default;
    bool HasGuild { get; }
    bool Shopping { get; }
    ulong BankAmount { get; }
    IShopperNpc TradingWithNpc { get; }

    byte MaxSoulPoints { get; }
    IVip Vip { get; }
    IPlayerChannel Channels { get; set; }
    IPlayerParty PlayerParty { get; set; }
    string GenderPronoun { get; }
    Gender Gender { get; }
    int PremiumTime { get; }
    IDictionary<SkillType, ISkill> Skills { get; }
    IDictionary<int, int> Storages { get; }

    bool CanSeeInspectionDetails { get; }

    /// <summary>
    ///     Indicates Skull showed on creature
    /// </summary>
    Skull Skull { get; }

    ulong GetTotalMoney(ICoinTypeStore coinTypeStore);

    uint ChooseToRemoveFromKnownSet(); //todo: looks like implementation detail

    /// <summary>
    ///     Checks if player knows creature with given id
    /// </summary>
    /// <param name="creatureId"></param>
    /// <returns></returns>
    bool KnowsCreatureWithId(uint creatureId);

    /// <summary>
    ///     Get skillType info
    /// </summary>
    /// <param name="skillType"></param>
    /// <returns></returns>
    ushort GetSkillLevel(SkillType skillType);

    /// <summary>
    ///     Changes player's fight mode
    /// </summary>
    /// <param name="fightMode"></param>
    void ChangeFightMode(FightMode fightMode);

    /// <summary>
    ///     Changes player's chase mode
    /// </summary>
    /// <param name="chaseMode"></param>
    void ChangeChaseMode(ChaseMode chaseMode);

    /// <summary>
    ///     Toogle Secure Mode
    /// </summary>
    /// <param name="secureMode"></param>
    void ChangeSecureMode(byte secureMode);

    byte GetSkillPercent(SkillType type);

    void AddKnownCreature(uint creatureId);

    /// <summary>
    ///     Checks if the player has specified mana points
    /// </summary>
    /// <param name="mana"></param>
    /// <returns></returns>
    bool HasEnoughMana(ushort mana);

    /// <summary>
    ///     Consume mana points
    /// </summary>
    /// <param name="mana"></param>
    void ConsumeMana(ushort mana);

    /// <summary>
    ///     Checks if the player has specified level points
    /// </summary>
    /// <returns></returns>
    bool HasEnoughLevel(ushort level);

    bool Logout(bool forced = false);
    ushort CalculateAttackPower(float attackRate, ushort attack);
    void LookAt(ITile tile);
    void LookAt(byte containerId, sbyte containerSlot);
    void LookAt(Slot slot);

    /// <summary>
    ///     Health and mana recovery
    /// </summary>
    void Recover();

    void HealMana(ushort increasing);

    bool Feed(IFood food);
    bool Feed(int duration);

    Result Use(IUsableOn item, ITile tile);
    Result Use(IUsableOn item, ICreature onCreature);
    void Use(IThing item);
    Result Use(IUsableOn item, IItem onItem);
    bool Login();

    bool CastSpell(string message);

    void SendMessageTo(ISociableCreature creature, SpeechType type, string message);
    void StartShopping(IShopperNpc npc);
    void StopShopping();
    bool Sell(IItemType item, byte amount, bool ignoreEquipped);
    void ReceivePayment(IEnumerable<IItem> coins, ulong total);
    bool CanReceiveInCashPayment(IEnumerable<IItem> coins);
    void ReceivePurchasedItems(INpc from, SaleContract saleContract, params IItem[] items);
    void WithdrawFromBank(ulong amount);
    void LoadBank(ulong amount);
    byte GetSkillTries(SkillType skillType);
    void AddSkillBonus(SkillType skillType, sbyte increase);
    void RemoveSkillBonus(SkillType skillType, sbyte decrease);
    sbyte GetSkillBonus(SkillType skill);
    void IncreaseSkillCounter(SkillType skill, long value);
    void DecreaseSkillCounter(SkillType skill, long value);
    void AddInventory(IInventory inventory);
    void Read(IReadable readable);
    void Write(IReadable readable, string text);
    void StopAllActions();
    Result<OperationResultList<IItem>> PickItemFromGround(IItem item, ITile tile, byte amount = 1);

    Result<OperationResultList<IItem>> MoveItem(IItem item, IHasItem source, IHasItem destination, byte amount,
        byte fromPosition,
        byte? toPosition);

    bool CanUseOutfit(IOutfit outFit);
    void SetAsHungry();
    void Use(IContainer item, byte openAtIndex);
    ushort GetRawSkillLevel(SkillType skillType);
    int GetStorageValue(int key);
    void AddOrUpdateStorageValue(int key, int value);
    bool HasSkull { get; }
    int NumberOfUnjustifiedKillsLastDay { get; }
    int NumberOfUnjustifiedKillsLastWeek { get; }
    int NumberOfUnjustifiedKillsLastMonth { get; }
    DateTime? SkullEndsAt { get; }
    event SkullUpdated OnSkullUpdated;
    void AddPlayerToEnemyList(IPlayer player);
    void AddPlayerToEnemyList(uint creatureId);
    bool PlayerIsOnEnemyList(uint creatureId);
    Skull GetSkull(IPlayer enemy);
    void SetSkull(Skull skull, DateTime? skullEndingDate = null);
    void RemoveSkull();
    void SetNumberOfKills(int killsInLastDay, int killsInLastWeek, int killsInLastMonth);
}