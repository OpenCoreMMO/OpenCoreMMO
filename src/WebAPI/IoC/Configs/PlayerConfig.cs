using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Web.API.IoC.Configs;

public class PlayerConfig
{
    public int? AccountId { get; set; }
    public ushort Level { get; set; }
    public uint Capacity { get; set; }
    public int Experience { get; set; }
    public Gender Gender { get; set; }
    public int WorldId { get; set; }
    public uint Health { get; set; }
    public uint MaxHealth { get; set; }
    public ushort Mana { get; set; }
    public ushort MaxMana { get; set; }
    public byte Soul { get; set; }
    public ushort Speed { get; set; }
    public string Name { get; set; }
    public FightMode FightMode { get; set; }
    public int LookType { get; set; }
    public int LookBody { get; set; }
    public int LookFeet { get; set; }
    public int LookHead { get; set; }
    public int LookLegs { get; set; }
    public int MagicLevel { get; set; }
    public byte Vocation { get; set; }
    public ChaseMode ChaseMode { get; set; }
    public byte MaxSoul { get; set; }
    public byte Group { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
    public int SkillAxe { get; set; }
    public int SkillDist { get; set; }
    public int SkillClub { get; set; }
    public int SkillSword { get; set; }
    public int SkillShielding { get; set; }
    public int SkillFist { get; set; }
    public int TownId { get; set; }
    public int SkillFishing { get; set; }
}
