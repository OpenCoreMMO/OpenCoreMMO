namespace NeoServer.Domain.Common.Creatures;

public enum Immunity : ushort
{
    Death = 1 << 0,
    Drown = 1 << 1,
    Drunkenness = 1 << 2,
    Earth = 1 << 3,
    Energy = 1 << 4,
    Fire = 1 << 5,
    Holy = 1 << 6,
    Ice = 1 << 7,
    Invisibility = 1 << 8,
    LifeDrain = 1 << 9,
    Paralysis = 1 << 10,
    Physical = 1 << 11,
    ManaDrain = 1 << 12,
    None = 0
}