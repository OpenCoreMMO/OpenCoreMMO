using NeoServer.Domain.Chat;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures.Players;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Npcs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Container;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Items.Items.UsableItems;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void ClosedContainer(IPlayer player, byte containerId, IContainer container);

public delegate void ClosedDepot(IPlayer player, byte containerId, Locker.Locker container);

public delegate void OpenedContainer(IPlayer player, byte containerId, IContainer container);

public delegate void CannotUseSpell(IPlayer player, ISpell spell, InvalidOperation error);

public delegate void LogIn(IPlayer player);

public delegate void AddToVipList(IPlayer player, uint vipPlayerId, string vipPlayerName);

public delegate void PlayerLoadVipList(IPlayer player, IEnumerable<(uint, string)> vipList);

public delegate void ReadText(IPlayer player, IReadable readable, string text);

public delegate void EquipItem(IPlayer player, IItem item, bool isCheck);

public delegate void DeEquipItem(IPlayer player, IItem item, bool isCheck);

public interface IPlayer : ICombatActor, ISociableCreature, IBankable
{
    ushort Level { get; }
    byte LevelPercent { get; }
    ushort MagicLevel { get; }

    uint Experience { get; }
    byte SoulPoints { get; }

    float FreeCapacity { get; }

    ushort StaminaMinutes { get; }
    bool IsLogoutBlocked { get; }

    FightMode FightMode { get; }
    ChaseMode ChaseMode { get; }
    PvpSecureMode SecureMode { get; }
    PlayerContainerList Containers { get; }

    ITown Town { get; set; }

    IInventory Inventory { get; }
    uint Mana { get; }

    /// <summary>
    ///     Spent mana consumption until magic level increase
    /// </summary>
    uint ManaSpent { get; }

    uint MaxMana { get; }
    SkillType SkillInUse { get; }
    bool CannotLogout { get; }
    uint Id { get; }
    bool HasDepotOpened { get; }
    uint TotalCapacity { get; }
    bool Recovering { get; }
    Vocation Vocation { get; }
    byte VocationType => Vocation?.VocationType ?? default;
    Group Group { get; set; }
    byte GroupId => Group?.Id ?? default;
    uint AccountId { get; init; }
    int WorldId { get; init; }
    Guild.Guild Guild { get; }
    ushort GuildId => Guild?.Id ?? default;
    bool HasGuild { get; }
    GuildRankInfo GuildRank { get; set; }
    string GuildNick { get; set; }
    bool Shopping { get; }
    IShopperNpc TradingWithNpc { get; }

    byte MaxSoulPoints { get; }
    IVip Vip { get; }
    PlayerChannel Channels { get; set; }
    PlayerParty PlayerParty { get; set; }
    string GenderPronoun { get; }
    Gender Gender { get; set; }
    int PremiumDays { get; }
    bool HasPremiumTime { get; }
    IDictionary<SkillType, Skill> Skills { get; }
    IDictionary<uint, int> Storages { get; }

    bool CanSeeInspectionDetails { get; }
    bool IsManaShieldEnabled { get; }
    void AddConditionSuppression(ConditionType conditionType);
    void RemoveConditionSuppression(ConditionType conditionType);
    int GetConditionSuppressionCount(ConditionType conditionType);

    /// <summary>
    ///     Indicates Skull showed on creature
    /// </summary>
    PlayerSkull PlayerSkull { get; }

    bool HasSkull { get; }
    int NumberOfUnjustifiedKillsLastDay { get; }
    int NumberOfUnjustifiedKillsLastWeek { get; }
    int NumberOfUnjustifiedKillsLastMonth { get; }
    DateTime? SkullEndsAt { get; }
    bool IsProtectionZoneBlocked { get; }
    Skull Skull { get; }
    float DamageFactor { get; }
    bool IsPacified { get; }
    DateTime? LastLogIn { get; }
    DateTime? LastLogOut { get; set; }
    bool IgnoreStamina { get; }
    bool IsPromoted { get; }
    void SetGuild(Guild.Guild guild);

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
    void ChangeSecureMode(PvpSecureMode secureMode);

    byte GetSkillPercent(SkillType type);

    void AddKnownCreature(uint creatureId);

    /// <summary>
    ///     Checks if the player has specified mana points
    /// </summary>
    /// <param name="mana"></param>
    /// <returns></returns>
    bool HasEnoughMana(uint mana);

    /// <summary>
    ///     Consume mana points
    /// </summary>
    /// <param name="mana"></param>
    void DecreaseMana(uint mana);

    /// <summary>
    ///     Update mana spent for magic level increase
    /// </summary>
    /// <param name="mana"></param>
    void UpdateManaSpent(uint manaCost);

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

    void IncreaseMana(uint increasing);

    bool Feed(Food food);
    bool Feed(int duration);

    Result Use(IUsableOn item, ITile tile);
    Result Use(IUsableOn item, ICreature onCreature);
    void Use(IThing item);
    Result Use(IUsableOn item, IItem onItem);
    bool Login();

    void SendMessageTo(ISociableCreature creature, SpeechType type, string message);
    void StartShopping(IShopperNpc npc);
    void StopShopping();
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

    bool CanUseOutfit(Outfit outFit);
    void SetAsHungry();
    void Use(IContainer item, byte openAtIndex);
    ushort GetRawSkillLevel(SkillType skillType);
    int GetStorageValue(uint key);
    void AddOrUpdateStorageValue(uint key, int value);
    Skull GetSkull(IPlayer enemy);
    void SetSkull(Skull skull, DateTime? skullEndingDate = null, IPlayer enemy = null);
    void RemoveSkull();
    void SetNumberOfKills(int killsInLastDay, int killsInLastWeek, int killsInLastMonth);
    void RemoveLogoutBlock();
    void SetProtectionZoneBlock();
    void RemoveProtectionZoneBlock();

    /// <summary>
    ///     Add infinite mana shield condition
    /// </summary>
    void EnableManaShield();

    /// <summary>
    ///     Remove mana shield condition
    /// </summary>
    void DisableManaShield();

    /// <summary>
    ///     Add mana shield condition
    /// </summary>
    /// <param name="duration"></param>
    void EnableManaShield(uint duration);

    void AddRegenerationBonus(RegenerationBonus regenerationBonus);
    void RemoveRegenerationBonus(RegenerationBonus regenerationBonus);
    void OnEquippedItem(IItem item);
    void OnUnequippedItem(IItem item);
    void PostSpellCast(ISpell spell);
    bool HasEnoughSoul(ushort soul);
    Result CanCastSpell(ISpell spell);
    Result CanPushCreature(ICreature creature, ITile destination);
    void ConsumeSoul(ushort soul);

    void PostAttack(CombatParameter combatParameter, IThing target, CombatResult damages);
    public void MoveToTemple();

    void RegenerateStamina();

    /// <summary>
    /// Sends a message as a yell to a list of listeners based on the provided yell settings.
    /// </summary>
    /// <param name="message">The message to be yelled.</param>
    /// <param name="listenersToYell">The list of creatures that will receive the yelled message.</param>
    /// <param name="yellSettings">The configuration settings that define the behavior and constraints of the yell action.</param>
    void Yell(string message, List<ICreature> listenersToYell, YellConfiguration yellSettings);
    void Whisper(string message, List<ICreature> listenersToWhisper);
    void StartCooldown(CooldownType cooldownType, uint cooldownTime);

    void HealSoul(ushort increasing);

    void AddEquipmentCondition(Slot slot, ICondition condition);
    void RemoveEquipmentCondition(Slot slot, ConditionType conditionType);
}