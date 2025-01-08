namespace NeoServer.Game.Common.Creatures.Structs;

public readonly record struct RegenerationBonus
{
    public ulong Id => ((ulong)Ticks << 16) | Gain;
    public required int Ticks { get; init; }
    public required ushort Gain { get; init; }
    public required RegenerationType Type { get; init; }
}

public enum RegenerationType
{
    Health,
    Mana
}