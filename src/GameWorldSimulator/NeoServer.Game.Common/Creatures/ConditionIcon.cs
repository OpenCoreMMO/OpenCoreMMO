namespace NeoServer.Game.Common.Creatures;

public enum ConditionIcon : uint
{
    None = 0,
    Poison = 1 << 0,
    Burn = 1 << 1,
    Energy = 1 << 2,
    Drunk = 1 << 3,
    ManaShield = 1 << 4,
    Paralyze = 1 << 5,
    Haste = 1 << 6,
    Swords = 1 << 7,
    Drowning = 1 << 8,
    Freezing = 1 << 9,
    Dazzled = 1 << 10,
    Cursed = 1 << 11,
    PartyBuff = 1 << 12,
    RedSwords = 1 << 13,
    Pigeon = 1 << 14
}