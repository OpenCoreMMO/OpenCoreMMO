using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Items;

public sealed class ItemTypeAttributeList : BaseAttributeList<ItemTypeAttribute>
{
    public Dictionary<SkillType, sbyte> SkillBonuses
    {
        get
        {
            var dictionary = new Dictionary<SkillType, sbyte>();

            foreach (var (attr, (value, _)) in _defaultAttributes)
            {
                var type = typeof(sbyte);
                var (skillType, bonus) = attr switch
                {
                    ItemTypeAttribute.SkillAxe => (SkillType.Axe, (sbyte)value),
                    ItemTypeAttribute.SkillClub => (SkillType.Club, (sbyte)value),
                    ItemTypeAttribute.SkillDistance => (SkillType.Distance, (sbyte)value),
                    ItemTypeAttribute.SkillFishing => (SkillType.Fishing, (sbyte)value),
                    ItemTypeAttribute.SkillFist => (SkillType.Fist, (sbyte)value),
                    ItemTypeAttribute.SkillShield => (SkillType.Shielding, (sbyte)value),
                    ItemTypeAttribute.SkillSword => (SkillType.Sword, (sbyte)value),
                    ItemTypeAttribute.Speed => (SkillType.Speed, (sbyte)value),
                    ItemTypeAttribute.MagicPoints => (SkillType.Magic, (sbyte)value),
                    _ => (SkillType.None, (sbyte)0)
                };

                if (skillType == SkillType.None || bonus == 0) continue;
                dictionary.TryAdd(skillType, bonus);
            }

            return dictionary;
        }
    }

    public Dictionary<DamageType, sbyte> DamageProtection
    {
        get
        {
            var dictionary = new Dictionary<DamageType, sbyte>();

            foreach (var (attr, (value, _)) in _defaultAttributes)
            {
                var type = typeof(sbyte);
                var (damage, protection) = attr switch
                {
                    ItemTypeAttribute.AbsorbPercentDeath => (DamageType.Death, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentEnergy => (DamageType.Energy, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentPhysical => (DamageType.Physical, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentPoison => (DamageType.Earth, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentFire => (DamageType.Fire, (sbyte)value),
                    ItemTypeAttribute.FieldAbsorbPercentFire => (DamageType.Fire, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentDrown => (DamageType.Drown, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentHoly => (DamageType.Holy, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentIce => (DamageType.Ice, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentManaDrain => (DamageType.ManaDrain, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentLifeDrain => (DamageType.LifeDrain, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentMagic => (DamageType.Elemental, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentAll => (DamageType.All, (sbyte)value),
                    ItemTypeAttribute.AbsorbPercentElements => (DamageType.Elemental, (sbyte)value),
                    _ => (DamageType.None, (sbyte)0)
                };

                if (damage == DamageType.None) continue;
                dictionary.TryAdd(damage, protection);
            }

            return dictionary;
        }
    }

    public FloorChangeDirection GetFloorChangeDirection()
    {
        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.FloorChange) ?? false)
        {
            var floorChange = GetAttribute(ItemTypeAttribute.FloorChange);

            return floorChange switch
            {
                "down" => FloorChangeDirection.Down,
                "north" => FloorChangeDirection.North,
                "south" => FloorChangeDirection.South,
                "southalt" => FloorChangeDirection.SouthAlternative,
                "west" => FloorChangeDirection.West,
                "east" => FloorChangeDirection.East,
                "eastalt" => FloorChangeDirection.EastAlternative,
                "up" => FloorChangeDirection.Up,
                _ => FloorChangeDirection.None
            };
        }

        return FloorChangeDirection.None;
    }

    public byte[] GetRequiredVocations()
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(ItemTypeAttribute.Vocation, out var value)) return (byte[])value.Item1;

        return default;
    }

    public EffectT GetEffect()
    {
        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.Effect) ?? false)
        {
            var effect = GetAttribute(ItemTypeAttribute.Effect);

            return effect switch
            {
                "teleport" => EffectT.BubbleBlue,
                "blueshimmer" => EffectT.GlitterBlue,
                "bluebubble" => EffectT.BubbleBlue,
                "greenbubble" => EffectT.GlitterGreen,
                _ => EffectT.None
            };
        }

        return EffectT.None;
    }

    public ushort GetTransformationItem()
    {
        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.TransformEquipTo) ?? false)
            return GetAttribute<ushort>(ItemTypeAttribute.TransformEquipTo);

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.TransformDequipTo) ?? false)
            return GetAttribute<ushort>(ItemTypeAttribute.TransformDequipTo);

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.TransformTo) ?? false)
            return GetAttribute<ushort>(ItemTypeAttribute.TransformTo);

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.ExpireTarget) ?? false)
            return GetAttribute<ushort>(ItemTypeAttribute.ExpireTarget);

        return 0;
    }

    public ushort GetDestructionItem()
    {
        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.DestroyTo) ?? false)
            return GetAttribute<ushort>(ItemTypeAttribute.DestroyTarget);

        return 0;
    }

    public ElementalDamage GetWeaponElementDamage()
    {
        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.ElementEarth) ?? false)
            return new ElementalDamage(DamageType.Earth, GetAttribute<byte>(ItemTypeAttribute.ElementEarth));

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.ElementEnergy) ?? false)
            return new ElementalDamage(DamageType.Energy, GetAttribute<byte>(ItemTypeAttribute.ElementEnergy));

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.ElementFire) ?? false)
            return new ElementalDamage(DamageType.Fire, GetAttribute<byte>(ItemTypeAttribute.ElementFire)); //todo

        if (_defaultAttributes?.ContainsKey(ItemTypeAttribute.ElementIce) ?? false)
            return new ElementalDamage(DamageType.Ice, GetAttribute<byte>(ItemTypeAttribute.ElementIce));

        return default;
    }
}