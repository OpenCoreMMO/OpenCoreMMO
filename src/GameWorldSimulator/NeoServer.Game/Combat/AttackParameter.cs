using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Formula;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Combat;

public readonly ref struct AttackInput(IThing aggressor, IThing target)
{
    public IThing Aggressor => aggressor;
    public IThing Target => target;
    public required AttackParameter Parameters { get; init; }
}


public struct AttackParameter
{
    public readonly record struct AttackCondition(byte DamageCount, int Interval)
    {
        public bool None => DamageCount == 0;
    }
    public byte Range { get; set; }
    public ushort MinDamage { get; set; }
    public ushort MaxDamage { get; set; }
    public MinMax Damage => new(MinDamage, MaxDamage);
    public DamageType DamageType { get; set; }
    public EffectT Effect { get; set; }
    public byte Radius { get; set; }
    public bool NeedTarget { get; set; }
    public byte Length { get; set; }
    public byte Spread { get; set; }
    public ShootType ShootType { get; set; }
    public required string Name { get; set; }
    public ExtraAttack ExtraAttack { get; set; }
    public CooldownType CooldownType { get; set; }
    public bool HasExtraAttack => ExtraAttack.MaxDamage > 0;
    public bool IsMagicalAttack { get; set; }
    public AreaAttackParameter Area { get; set; }
    public bool IsAttackInArea => !Area.IsEmpty;
    public bool BlockArmor { get; set; }
    public DamageFormula Formula { get; set; }
    public bool AmmoCanCauseMiss { get; set; }
    public ushort CreateItemId { get; set; }
    public AttackCondition Condition { get; set; }
    public int Cooldown { get; set; }
}

public readonly struct ExtraAttack
{
    public ushort MinDamage { get; init; }
    public ushort MaxDamage { get; init; }
    public DamageType DamageType { get; init; }
    public bool IsMagicalAttack { get; init; }
}
public struct AreaAttackParameter
{
    public void SetArea(Coordinate[] coordinates, EffectT effect, bool excludeOrigin = false)
    {
        Coordinates = coordinates;
        Effect = effect;
        ExcludeOrigin = false;
    }
    
    public Coordinate[] Coordinates { get; private set; }
    public EffectT Effect { get; private set; }
    public bool ExcludeOrigin { get; private set; }
    public bool IsEmpty => (Coordinates?.Length ?? 0) == 0;
}

public readonly struct AffectedLocation2(Coordinate coordinate, bool missed)
{
    public Coordinate Point => coordinate;
    public bool Missed => missed;
}