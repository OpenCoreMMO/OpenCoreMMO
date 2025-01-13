using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Data.Entities;

public class PlayerDeathEntity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public DateTime DeathDateTime { get; set; }
    public string DeathLocation { get; set; }
    public int Level { get; set; }
    public int ExperienceLost { get; set; }
    public int SkillAxeLost { get; set; }
    public int SkillSwordLost { get; set; }
    public int SkillClubLost { get; set; }
    public int SkillFistLost { get; set; }
    public int SkillDistanceLost { get; set; }
    public int SkillMagicLevelLost { get; set; }
    public int SkillFishingLost { get; set; }
    public bool Unjustified { get; set; }
    public ICollection<PlayerDeathKillerEntity> Killers { get; set; }
}