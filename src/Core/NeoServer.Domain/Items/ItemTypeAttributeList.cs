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

            foreach (var (attr, (value, list)) in _defaultAttributes)
            {
                var type = typeof(sbyte);
                var (skillType, bonus) = attr switch
                {
                    ItemTypeAttribute.SkillAxe => (SkillType.Axe, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillClub => (SkillType.Club, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillDistance => (SkillType.Distance, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillFishing => (SkillType.Fishing, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillFist => (SkillType.Fist, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillShield => (SkillType.Shielding, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.SkillSword => (SkillType.Sword, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.Speed => (SkillType.Speed, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.MagicPoints => (SkillType.Magic, Convert.ChangeType(value, type)),
                    _ => (SkillType.None, (byte)0)
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
                    ItemTypeAttribute.AbsorbPercentDeath => (DamageType.Death, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentEnergy => (DamageType.Energy, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentPhysical => (DamageType.Physical, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentPoison => (DamageType.Earth, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentFire => (DamageType.Fire, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.FieldAbsorbEercentFire => (DamageType.FireField, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentDrown => (DamageType.Drown, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentHoly => (DamageType.Holy, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentIce => (DamageType.Ice, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentManaDrain => (DamageType.ManaDrain, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentLifeDrain => (DamageType.LifeDrain, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentMagic => (DamageType.Elemental, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentAll => (DamageType.All, Convert.ChangeType(value, type)),
                    ItemTypeAttribute.AbsorbPercentElements => (DamageType.Elemental, Convert.ChangeType(value, type)),
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