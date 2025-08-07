namespace NeoServer.Domain.Common.Item;

public enum DamageOrigin : byte
{
    None = default,
    Condition,
    Spell,
    Melee,
    Ranged
}