using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Effects.Parsers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class MagicField : BaseItem
{
    public MagicField(IItemType type, Location location) : base(type, location)
    {
    }

    public IThing Creator { get; set; }

    private byte DamageCount => Metadata.Attributes.GetInnerAttributes(ItemAttribute.Field)
        ?.GetAttribute<byte>(ItemAttribute.Count) ?? 0;

    private DamageType DamageType => DamageTypeParser.Parse(Metadata.Attributes.GetAttribute(ItemAttribute.Field));

    private uint Interval =>
        Metadata.Attributes.GetInnerAttributes(ItemAttribute.Field)?.GetAttribute<uint>(ItemAttribute.Ticks) ??
        10000;

    private MinMax Damage
    {
        get
        {
            var attributes = Metadata.Attributes.GetInnerAttributes(ItemAttribute.Field);
            if (attributes is null) return new MinMax();

            var values = attributes.GetAttributeArray(ItemAttribute.Damage);

            if ((values?.Length ?? 0) < 2) return new MinMax(0, 0);

            return new MinMax(Math.Min((ushort)values[0], (ushort)values[1]),
                Math.Max((ushort)values[0], (ushort)values[1]));
        }
    }

    public void CauseDamage(ICreature toCreature)
    {
        if (toCreature is not ICombatActor actor) return;

        var damages = Damage;

        if (damages.Max == 0) return;
        var conditionType = DamageType.ToConditionType();
        actor.TakeDamage(this,
            new CombatDamage((ushort)damages.Max, DamageType) { Effect = DamageEffectParser.Parse(DamageType) });

        if (actor.HasCondition(conditionType, out var condition) && condition is ConditionDamage damageCondition)
        {
            if (DamageCount == 0) damageCondition.Start(toCreature, (ushort)damages.Min, (ushort)damages.Max);
            else damageCondition.Restart(DamageCount);
        }
        else
        {
            if (DamageCount == 0)
                actor.AddCondition(new ConditionDamage(this, conditionType, Interval, (ushort)damages.Min,
                    (ushort)damages.Max));
            else
                actor.AddCondition(new ConditionDamage(this, conditionType, Interval, DamageCount,
                    (ushort)damages.Min));
        }
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.MagicField;
    }
}