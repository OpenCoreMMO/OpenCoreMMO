namespace NeoServer.Domain.Common.Creatures;

public enum SkillType : byte
{
    Fist = 0,
    Club = 1,
    Sword = 2,
    Axe = 3,
    Distance = 4,
    Shielding = 5,
    Fishing = 6,
    Magic = 7,
    Level = 8,
    Speed = 9,
    None
}

//todo: use this
public enum SpecialSkillType : byte
{
    CriticalHitChance = 0,
    CriticalHitAmount = 1,
    LifeLeechChance = 2,
    LifeLeechAmount = 3,
    ManaLeechChance = 4,
    ManaLeechAmount = 5,
    None
}