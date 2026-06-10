using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Modes;

namespace NeoServer.Data.Entities;

public sealed class PlayerEntity
{
    public PlayerEntity()
    {
        PlayerInventoryItems = new List<PlayerInventoryItemEntity>();
        PlayerDepotItems = new List<PlayerDepotItemEntity>();
        PlayerItems = new List<PlayerItemEntity>();
        PlayerDepotItems = new List<PlayerDepotItemEntity>();
        Deaths = new List<PlayerDeathEntity>();
    }

    public int Id { get; set; }
    public int AccountId { get; set; }
    public int TownId { get; set; }
    public string Name { get; set; }
    public byte Group { get; set; } //1 = player
    public uint Capacity { get; set; }
    public ushort Level { get; set; }
    public uint Mana { get; set; }
    public uint MaxMana { get; set; }
    public uint ManaSpent { get; set; }
    public uint Health { get; set; }
    public uint MaxHealth { get; set; }
    public byte Soul { get; set; }
    public byte MaxSoul { get; set; }
    public ushort StaminaMinutes { get; set; }
    public bool Online { get; set; }
    public byte LightLevel { get; set; }
    public byte LightColor { get; set; }

    public int LookAddons { get; set; }
    public int LookBody { get; set; }
    public int LookFeet { get; set; }
    public int LookHead { get; set; }
    public int LookLegs { get; set; }
    public int LookType { get; set; }

    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
    public int SkillFist { get; set; }
    public double SkillFistTries { get; set; }

    public int SkillClub { get; set; }
    public double SkillClubTries { get; set; }

    public int SkillSword { get; set; }
    public double SkillSwordTries { get; set; }

    public int SkillAxe { get; set; }
    public double SkillAxeTries { get; set; }

    public int SkillDist { get; set; }
    public double SkillDistTries { get; set; }

    public int SkillShielding { get; set; }
    public double SkillShieldingTries { get; set; }

    public int SkillFishing { get; set; }
    public double SkillFishingTries { get; set; }

    public int MagicLevel { get; set; }
    public double MagicLevelTries { get; set; }
    public double Experience { get; set; }
    public ulong BankAmount { get; set; }

    public ChaseMode ChaseMode { get; set; }
    public FightMode FightMode { get; set; }
    public Gender Gender { get; set; }
    public byte Vocation { get; set; }
    public int RemainingRecoverySeconds { get; set; }

    public Skull Skull { get; set; }
    public DateTime? SkullEndsAt { get; set; }
    public DateTime? LastLogOut { get; set; }
    public DateTime? LastLogIn { get; set; }
    public AccountEntity Account { get; set; }
    public ICollection<PlayerDeathEntity> Deaths { get; set; }
    public ICollection<PlayerDeathEntity> KillsLastMonth { get; set; }

    public ICollection<PlayerItemEntity> PlayerItems { get; set; }
    public ICollection<PlayerDepotItemEntity> PlayerDepotItems { get; set; }
    public ICollection<PlayerMailItemEntity> PlayerMailItems { get; set; }
    public ICollection<PlayerInventoryItemEntity> PlayerInventoryItems { get; set; }
    public GuildMembershipEntity GuildMember { get; set; }
    public WorldEntity World { get; set; }
    public int WorldId { get; set; }
    public ICollection<PlayerStorageEntity> PlayerStorages { get; set; }
    
    /// <summary>
    ///     JSON-serialized array of player condition objects (e.g., regeneration, haste, paralyze).
    ///     Deserialized at login and serialized on save.
    /// </summary>
    public List<ICondition> Conditions { get; set; }
}