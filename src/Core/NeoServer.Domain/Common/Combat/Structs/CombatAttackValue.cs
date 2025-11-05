using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Combat.Structs;

public ref struct CombatAttackValue
{
    public CombatAttackValue(ushort minDamage, ushort maxDamage, DamageType damageType) : this()
    {
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        DamageType = damageType;
    }

    public CombatAttackValue(ushort minDamage, ushort maxDamage, byte range, DamageType damageType)
    {
        Range = range;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        DamageType = damageType;
        DamageEffect = EffectT.None;
    }

    public byte Range { get; set; }
    public ushort MinDamage { get; set; }
    public ushort MaxDamage { get; set; }
    public DamageType DamageType { get; set; }
    public EffectT DamageEffect { get; set; }
}

public struct CombatAttackResult
{
    public CombatAttackResult(ShootType shootType) : this()
    {
        ShootType = shootType;
    }

    public CombatAttackResult(DamageType damageType) : this()
    {
        DamageType = damageType;
    }

    public bool Missed { get; set; }
    public ShootType ShootType { get; set; }
    public DamageType DamageType { get; set; }
    public EffectT EffectT { get; set; }
    public AffectedLocation[] Area { get; set; }
    public static CombatAttackResult None => new();

    public void SetArea(Coordinate[] coordinates)
    {
        if (coordinates is null)
        {
            Area = [];
            return;
        }

        Area = new AffectedLocation[coordinates.Length];

        var i = 0;
        foreach (var coordinate in coordinates) Area[i++] = new AffectedLocation(coordinate);
    }
}

public class AffectedLocation
{
    public AffectedLocation(Coordinate coordinate)
    {
        Point = coordinate;
        Missed = false;
    }

    public Coordinate Point { get; }
    public bool Missed { get; private set; }

    public void MarkAsMissed()
    {
        Missed = true;
    }
}