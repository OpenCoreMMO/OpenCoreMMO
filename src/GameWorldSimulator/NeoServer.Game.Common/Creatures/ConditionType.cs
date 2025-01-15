namespace NeoServer.Game.Common.Creatures;

public enum ConditionType : uint
{
    None,
    Poison = 1 << 0,
    Fire = 1 << 1,
    Energy = 1 << 2,
    Bleeding = 1 << 3,
    Haste = 1 << 4,
    Paralyze = 1 << 5,
    Outfit = 1 << 6,
    Invisible = 1 << 7,
    Light = 1 << 8,
    Manashield = 1 << 9,
    LogoutBlock = 1 << 10,
    Drunk = 1 << 11,
    ExhaustWeapon = 1 << 12, // unused
    Regeneration = 1 << 13,
    Soul = 1 << 14,
    Drown = 1 << 15,
    Muted = 1 << 16,
    ChannelMutedTicks = 1 << 17,
    YellTicks = 1 << 18,
    Attributes = 1 << 19,
    Freezing = 1 << 20,
    Dazzled = 1 << 21,
    Cursed = 1 << 22,
    ExhaustCombat = 1 << 23, // unused
    ExhaustHeal = 1 << 24, // unused
    Pacified = 1 << 25,
    Illusion = 1 << 26,
    Hungry = 1 << 27,
    Strengthened = 1 << 28
}