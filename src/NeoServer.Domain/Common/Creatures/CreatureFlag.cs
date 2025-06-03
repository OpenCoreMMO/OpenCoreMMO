namespace NeoServer.Domain.Common.Creatures;

public enum CreatureFlag : uint
{
    None = 0,
    KickBoxes = 1 << 1,
    KickCreatures = 1 << 2,
    SeeInvisible = 1 << 3,
    Unpushable = 1 << 4,
    DistanceFighting = 1 << 5,
    NoSummon = 1 << 6,
    NoIllusion = 1 << 7,
    NoConvince = 1 << 8,
    NoBurning = 1 << 9,
    NoPoison = 1 << 10,
    NoEnergy = 1 << 11,
    NoParalyze = 1 << 12,
    NoHit = 1 << 13,
    NoLifeDrain = 1 << 14
}