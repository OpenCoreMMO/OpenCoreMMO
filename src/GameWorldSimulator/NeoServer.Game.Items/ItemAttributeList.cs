using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;

namespace NeoServer.Game.Items;

public sealed class ItemAttributeList : IItemAttributeList
{
    private readonly IDictionary<ItemAttribute, (dynamic, IItemAttributeList)> _defaultAttributes;
    private IDictionary<string, (dynamic, IItemAttributeList)> customAttributes;

    public ItemAttributeList()
    {
        _defaultAttributes = new Dictionary<ItemAttribute, (dynamic, IItemAttributeList)>();
    }

    private IDictionary<string, (dynamic, IItemAttributeList)> _customAttributes
    {
        get
        {
            customAttributes ??= new Dictionary<string, (dynamic, IItemAttributeList)>(StringComparer
                .InvariantCultureIgnoreCase);
            return customAttributes;
        }
    }

    public void SetCustomAttribute(string attribute, int attributeValue)
    {
        _customAttributes[attribute] = (attributeValue, null);
    }

    public void SetCustomAttribute(string attribute, IConvertible attributeValue)
    {
        _customAttributes[attribute] = (attributeValue, null);
    }

    public void SetCustomAttribute(string attribute, dynamic values)
    {
        _customAttributes[attribute] = (values, null);
    }

    public void SetCustomAttribute(string attribute, IConvertible attributeValue, IItemAttributeList attrs)
    {
        _customAttributes[attribute] = (attributeValue, attrs);
    }

    public void SetAttribute(ItemAttribute attribute, IConvertible attributeValue)
    {
        if (attribute is ItemAttribute.ActionId or ItemAttribute.UniqueId) return;

        _defaultAttributes.AddOrUpdate(attribute, (attributeValue, null));
    }

    public void SetAttribute(IDictionary<ItemAttribute, IConvertible> attributeValues)
    {
        if (attributeValues.IsNull()) return;

        foreach (var (key, value) in attributeValues) SetAttribute(key, value);
    }

    public void SetAttribute(ItemAttribute attribute, dynamic values)
    {
        if (attribute is ItemAttribute.ActionId or ItemAttribute.UniqueId) return;
        _defaultAttributes[attribute] = (values, null);
    }

    public void SetAttribute(ItemAttribute attribute, IConvertible attributeValue, IItemAttributeList attrs)
    {
        if (attribute is ItemAttribute.ActionId or ItemAttribute.UniqueId) return;
        _defaultAttributes[attribute] = (attributeValue, attrs);
    }

    public bool HasAttribute(ItemAttribute attribute)
    {
        return _defaultAttributes.ContainsKey(attribute);
    }

    public bool HasAttribute(string attribute)
    {
        return _customAttributes.ContainsKey(attribute);
    }

    public T GetAttribute<T>(ItemAttribute attribute) where T : struct
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value))
            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);

        return default;
    }

    public bool TryGetAttribute<T>(ItemAttribute attribute, out T attrValue) where T : struct
    {
        attrValue = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public bool TryGetAttribute(ItemAttribute attribute, out string attrValue)
    {
        attrValue = default;

        if (_defaultAttributes is null) return false;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = value.Item1;
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public bool TryGetAttribute<T>(string attribute, out T attrValue)
    {
        attrValue = default;

        if (_customAttributes is null) return false;

        if (!_customAttributes.TryGetValue(attribute, out var value)) return false;

        try
        {
            attrValue = (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }
        catch
        {
            attrValue = default;
        }

        return true;
    }

    public string GetAttribute(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public T GetAttribute<T>(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value))
            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);

        return default;
    }

    public string GetAttribute(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public dynamic[] GetAttributeArray(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return default;
        if (value.Item1 is not Array) return new[] { value.Item1 };
        
        var pool = ArrayPool<dynamic>.Shared;
        dynamic[] newArray = pool.Rent(value.Item1.Length);

        for (var i = 0; i < value.Item1.Length; i++) newArray[i] = value.Item1[i];

        pool.Return(newArray);

        int count = value.Item1.Length;
        return newArray[..count];
    }

    public T[] GetAttributeArray<T>(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (!_defaultAttributes.TryGetValue(attribute, out var value)) return default;

        if (value.Item1 is not Array) return new[] { (T)value.Item1 };

        var pool = ArrayPool<T>.Shared;
        T[] newArray = pool.Rent(value.Item1.Length);

        for (var i = 0; i < value.Item1.Length; i++) newArray[i] = (T)value.Item1[i];

        pool.Return(newArray);

        int count = value.Item1.Length;
        return newArray[..count];
    }

    public dynamic[] GetAttributeArray(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (value.Item1 is not Array) return default;
            
            var pool = ArrayPool<dynamic>.Shared;
            dynamic[] newArray = pool.Rent(value.Item1.Length);

            for (var i = 0; i < value.Item1.Length; i++) newArray[i] = value.Item1[i];

            pool.Return(newArray);

            int count = value.Item1.Length;
            return newArray[..count];
        }

        return default;
    }

    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>()
    {
        if (_defaultAttributes is null && _customAttributes is null) return default;

        var dictionary = new Dictionary<TKey, TValue>();

        if (_defaultAttributes is not null)
            foreach (var item in _defaultAttributes)
                dictionary.Add((TKey)Convert.ChangeType(item.Key, typeof(TKey), CultureInfo.InvariantCulture),
                    (TValue)item.Value.Item1);
        if (_customAttributes is not null)
            foreach (var item in _customAttributes)
                dictionary.Add((TKey)Convert.ChangeType(item.Key, typeof(TKey), CultureInfo.InvariantCulture),
                    (TValue)item.Value.Item1);
        return dictionary;
    }

    public IItemAttributeList GetInnerAttributes(ItemAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return value.Item2;

        return default;
    }

    public FloorChangeDirection GetFloorChangeDirection()
    {
        if (_defaultAttributes?.ContainsKey(ItemAttribute.FloorChange) ?? false)
        {
            var floorChange = GetAttribute(ItemAttribute.FloorChange);

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

        if (_defaultAttributes.TryGetValue(ItemAttribute.Vocation, out var value)) return (byte[])value.Item1;

        return default;
    }

    public EffectT GetEffect()
    {
        if (_defaultAttributes?.ContainsKey(ItemAttribute.Effect) ?? false)
        {
            var effect = GetAttribute(ItemAttribute.Effect);

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
        if (_defaultAttributes?.ContainsKey(ItemAttribute.TransformEquipTo) ?? false)
            return GetAttribute<ushort>(ItemAttribute.TransformEquipTo);

        if (_defaultAttributes?.ContainsKey(ItemAttribute.TransformDequipTo) ?? false)
            return GetAttribute<ushort>(ItemAttribute.TransformDequipTo);

        if (_defaultAttributes?.ContainsKey(ItemAttribute.TransformTo) ?? false)
            return GetAttribute<ushort>(ItemAttribute.TransformTo);

        if (_defaultAttributes?.ContainsKey(ItemAttribute.ExpireTarget) ?? false)
            return GetAttribute<ushort>(ItemAttribute.ExpireTarget);

        return 0;
    }

    public Tuple<DamageType, byte> GetWeaponElementDamage()
    {
        if (_defaultAttributes?.ContainsKey(ItemAttribute.ElementEarth) ?? false)
            return new Tuple<DamageType, byte>(DamageType.Earth, GetAttribute<byte>(ItemAttribute.ElementEarth));

        if (_defaultAttributes?.ContainsKey(ItemAttribute.ElementEnergy) ?? false)
            return new Tuple<DamageType, byte>(DamageType.Energy, GetAttribute<byte>(ItemAttribute.ElementEnergy));

        if (_defaultAttributes?.ContainsKey(ItemAttribute.ElementFire) ?? false)
            return new Tuple<DamageType, byte>(DamageType.FireField,
                GetAttribute<byte>(ItemAttribute.ElementFire)); //todo

        if (_defaultAttributes?.ContainsKey(ItemAttribute.ElementIce) ?? false)
            return new Tuple<DamageType, byte>(DamageType.Ice, GetAttribute<byte>(ItemAttribute.ElementIce));

        return null;
    }

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
                    ItemAttribute.SkillAxe => (SkillType.Axe, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillClub => (SkillType.Club, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillDistance => (SkillType.Distance, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillFishing => (SkillType.Fishing, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillFist => (SkillType.Fist, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillShield => (SkillType.Shielding, Convert.ChangeType(value, type)),
                    ItemAttribute.SkillSword => (SkillType.Sword, Convert.ChangeType(value, type)),
                    ItemAttribute.Speed => (SkillType.Speed, Convert.ChangeType(value, type)),
                    ItemAttribute.MagicPoints => (SkillType.Magic, Convert.ChangeType(value, type)),
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
                    ItemAttribute.AbsorbPercentDeath => (DamageType.Death, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentEnergy => (DamageType.Energy, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentPhysical => (DamageType.Physical, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentPoison => (DamageType.Earth, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentFire => (DamageType.Fire, Convert.ChangeType(value, type)),
                    ItemAttribute.FieldAbsorbEercentFire => (DamageType.FireField, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentDrown => (DamageType.Drown, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentHoly => (DamageType.Holy, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentIce => (DamageType.Ice, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentManaDrain => (DamageType.ManaDrain, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentLifeDrain => (DamageType.LifeDrain, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentMagic => (DamageType.Elemental, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentAll => (DamageType.All, Convert.ChangeType(value, type)),
                    ItemAttribute.AbsorbPercentElements => (DamageType.Elemental, Convert.ChangeType(value, type)),
                    _ => (DamageType.None, (sbyte)0)
                };

                if (damage == DamageType.None) continue;
                dictionary.TryAdd(damage, protection);
            }

            return dictionary;
        }
    }
}