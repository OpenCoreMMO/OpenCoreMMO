using System.Buffers;
using System.Globalization;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Items;

public sealed class ItemTypeAttributeList
{
    private readonly IDictionary<ItemTypeAttribute, (dynamic, ItemTypeAttributeList)> _defaultAttributes;
    private IDictionary<string, (dynamic, ItemTypeAttributeList)> customAttributes;

    public ItemTypeAttributeList()
    {
        _defaultAttributes = new Dictionary<ItemTypeAttribute, (dynamic, ItemTypeAttributeList)>();
    }

    private IDictionary<string, (dynamic, ItemTypeAttributeList)> _customAttributes
    {
        get
        {
            customAttributes ??= new Dictionary<string, (dynamic, ItemTypeAttributeList)>(StringComparer
                .InvariantCultureIgnoreCase);
            return customAttributes;
        }
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

    public void SetCustomAttribute(string attribute, IConvertible attributeValue, ItemTypeAttributeList attrs)
    {
        _customAttributes[attribute] = (attributeValue, attrs);
    }

    public void SetAttribute(ItemTypeAttribute attribute, IConvertible attributeValue)
    {
        _defaultAttributes.AddOrUpdate(attribute, (attributeValue, null));
    }

    public void SetAttribute(IDictionary<ItemTypeAttribute, IConvertible> attributeValues)
    {
        if (attributeValues.IsNull()) return;

        foreach (var (key, value) in attributeValues) SetAttribute(key, value);
    }

    public void SetAttribute(ItemTypeAttribute attribute, dynamic values)
    {
        _defaultAttributes[attribute] = (values, null);
    }

    public void SetAttribute(ItemTypeAttribute attribute, IConvertible attributeValue, ItemTypeAttributeList attrs)
    {
        _defaultAttributes[attribute] = (attributeValue, attrs);
    }

    public bool HasAttribute(ItemTypeAttribute attribute)
    {
        return _defaultAttributes.ContainsKey(attribute);
    }

    public bool HasAttribute(string attribute)
    {
        return _customAttributes.ContainsKey(attribute);
    }

    public T GetAttribute<T>(ItemTypeAttribute attribute) where T : struct
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value))
            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);

        return default;
    }

    public bool TryGetAttribute<T>(ItemTypeAttribute attribute, out T attrValue) where T : struct
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

    public bool TryGetAttribute(ItemTypeAttribute attribute, out string attrValue)
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

    public string GetAttribute(ItemTypeAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public T GetAttribute<T>(string attribute)
    {
        if (_customAttributes is null) return default;


        if (_customAttributes.TryGetValue(attribute, out var value))
        {
            if (IsNullable(value.Item1)) return (T)value.Item1;

            return (T)Convert.ChangeType(value.Item1, typeof(T), CultureInfo.InvariantCulture);
        }

        return default;
    }

    public string GetAttribute(string attribute)
    {
        if (_customAttributes is null) return default;

        if (_customAttributes.TryGetValue(attribute, out var value)) return (string)value.Item1;

        return default;
    }

    public dynamic[] GetAttributeArray(ItemTypeAttribute attribute)
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

    public T[] GetAttributeArray<T>(ItemTypeAttribute attribute)
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

    public ItemTypeAttributeList GetInnerAttributes(ItemTypeAttribute attribute)
    {
        if (_defaultAttributes is null) return default;

        if (_defaultAttributes.TryGetValue(attribute, out var value)) return value.Item2;

        return default;
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

    private static bool IsNullable(dynamic value)
    {
        if (value == null)
            return true; // null itself is always nullable

        var type = ((object)value).GetType();

        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    public ItemTypeAttributeList Clone()
    {
        var clone = new ItemTypeAttributeList();

        foreach (var kvp in _defaultAttributes)
        {
            clone._defaultAttributes[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2?.Clone());
        }

        if (customAttributes != null)
        {
            foreach (var kvp in customAttributes)
            {
                clone._customAttributes[kvp.Key] = (kvp.Value.Item1, kvp.Value.Item2?.Clone());
            }
        }

        return clone;
    }
}