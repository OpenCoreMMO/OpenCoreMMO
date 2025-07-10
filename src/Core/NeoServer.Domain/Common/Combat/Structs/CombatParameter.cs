using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Common.Combat.Structs;

public class AttackInput
{
    public AttackInput(IThing aggressor, IThing target, CombatParameter parameter)
    {
        Aggressor = aggressor;
        Target = target;
        Parameters = parameter;
    }

    public AttackInput(IThing aggressor, CombatParameter parameter)
    {
        Aggressor = aggressor;
        Parameters = parameter;
    }

    public IThing Aggressor { get; }
    public IThing Target { get; }
    public CombatParameter Parameters { get; init; }
    public bool HasTarget => Target is not null;
}

public readonly struct CombatContext
{
    public bool InfiniteAmmo { get; init; }
    public bool InfiniteThrowingWeapon { get; init; }
    public CombatParameter CombatParameters { get; init; }
}

public class CombatParameter
{
    public bool UsingWeapon { get; set; }
    public byte? Range { get; set; }
    public ushort MinDamage { get; set; }
    public ushort MaxDamage { get; set; }
    public DamageType DamageType { get; set; }
    public EffectT Effect { get; set; }

    public byte Radius { get; set; }
    public byte Length { get; set; }
    public byte Spread { get; set; }
    public ShootType ShootType { get; set; }

    //public required string Name { get; set; }
    public ExtraAttack ExtraAttack { get; set; }
    public CooldownType CooldownType { get; set; }
    public Guid CooldownId { get; set; }
    public bool HasExtraAttack => ExtraAttack.MaxDamage > 0;
    public bool IsMagicalAttack { get; set; }
    public bool IsAttackInArea => CoordinateArea?.Length > 0 || Area?.Length > 0;
    public bool BlockArmor { get; set; }
    public ushort CreateItemId { get; set; }
    public AttackCondition Condition { get; set; }
    public uint CooldownDuration { get; set; }
    public byte? HitChance { get; set; }
    public byte[,] Area { get; set; }
    public Coordinate[] CoordinateArea { get; set; }
    public bool NeedDirection { get; set; }

    public (FormulaType Formula, Func<IPlayer, int, int, decimal, MinMax> Callback) DamageFormula { get; set; } =
        (Formula: FormulaType.None, null);

    public bool FieldAttack { get; set; }
    public List<ICondition> Conditions { get; set; } = new();

    public void SetMinMaxDamage(MinMax minMaxDamage)
    {
        MinDamage = (ushort)minMaxDamage.Min;
        MaxDamage = (ushort)minMaxDamage.Max;
    }

    public void SetExtraAttack(ExtraAttack extraAttack)
    {
        ExtraAttack = extraAttack;
    }

    public class AttackCondition
    {
        public AttackCondition(ConditionType type, uint duration)
        {
            Type = type;
            Duration = duration;
        }

        public ConditionType Type { get; }
        public uint Duration { get; set; }
        public object Value { get; set; }
    }
}

public enum FormulaType
{
    None,
    MagicLevel,
    Skill,
    Damage
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