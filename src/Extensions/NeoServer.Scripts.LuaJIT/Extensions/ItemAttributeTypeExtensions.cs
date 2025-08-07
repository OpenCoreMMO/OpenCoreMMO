using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Extensions;

public static class ItemAttributeTypeExtensions
{
    public static ItemAttribute ToItemAttribute(this ItemAttributeType value)
    {
        return value switch
        {
            ItemAttributeType.ITEM_ATTRIBUTE_ACTIONID => ItemAttribute.ActionId,
            ItemAttributeType.ITEM_ATTRIBUTE_UNIQUEID => ItemAttribute.UniqueId,
            ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION => ItemAttribute.Description,
            ItemAttributeType.ITEM_ATTRIBUTE_TEXT => ItemAttribute.Text,
            ItemAttributeType.ITEM_ATTRIBUTE_DATE => ItemAttribute.Date,
            ItemAttributeType.ITEM_ATTRIBUTE_WRITER => ItemAttribute.Writer,
            ItemAttributeType.ITEM_ATTRIBUTE_NAME => ItemAttribute.Name,
            ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE => ItemAttribute.Article,
            ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME => ItemAttribute.PluralName,
            ItemAttributeType.ITEM_ATTRIBUTE_WEIGHT => ItemAttribute.Weight,
            ItemAttributeType.ITEM_ATTRIBUTE_ATTACK => ItemAttribute.Attack,
            ItemAttributeType.ITEM_ATTRIBUTE_DEFENSE => ItemAttribute.Defense,
            ItemAttributeType.ITEM_ATTRIBUTE_EXTRADEFENSE => ItemAttribute.ExtraDefense,
            ItemAttributeType.ITEM_ATTRIBUTE_ARMOR => ItemAttribute.Armor,
            ItemAttributeType.ITEM_ATTRIBUTE_HITCHANCE => ItemAttribute.HitChance,
            ItemAttributeType.ITEM_ATTRIBUTE_SHOOTRANGE => ItemAttribute.ShootRange,
            ItemAttributeType.ITEM_ATTRIBUTE_OWNER => ItemAttribute.Owner,
            ItemAttributeType.ITEM_ATTRIBUTE_WRAPID => ItemAttribute.WrapId,
            ItemAttributeType.ITEM_ATTRIBUTE_DURATION => ItemAttribute.Duration,
            ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE => ItemAttribute.DecayState,
            ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER => ItemAttribute.CorpseOwner,
            ItemAttributeType.ITEM_ATTRIBUTE_CHARGES => ItemAttribute.Charges,
            ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE => ItemAttribute.FluidType,
            ItemAttributeType.ITEM_ATTRIBUTE_DOORID => ItemAttribute.DoorId,
            ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO => ItemAttribute.DecayTo,
            ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM => ItemAttribute.StoreItem,
            ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED => ItemAttribute.AttackSpeed,
            ItemAttributeType.ITEM_ATTRIBUTE_CUSTOM => ItemAttribute.Custom,
            _ => ItemAttribute.None
        };
    }

    public static ItemTypeAttribute ToItemTypeAttribute(this ItemAttributeType value)
    {
        return value switch
        {
            //ItemAttributeType.ITEM_ATTRIBUTE_ACTIONID => ItemTypeAttribute.ActionId,
            //ItemAttributeType.ITEM_ATTRIBUTE_UNIQUEID => ItemTypeAttribute.UniqueId,
            ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION => ItemTypeAttribute.Description,
            ItemAttributeType.ITEM_ATTRIBUTE_TEXT => ItemTypeAttribute.Text,
            ItemAttributeType.ITEM_ATTRIBUTE_DATE => ItemTypeAttribute.WrittenDate,
            ItemAttributeType.ITEM_ATTRIBUTE_WRITER => ItemTypeAttribute.WrittenBy,
            ItemAttributeType.ITEM_ATTRIBUTE_NAME => ItemTypeAttribute.Name,
            ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE => ItemTypeAttribute.Article,
            ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME => ItemTypeAttribute.PluralName,
            ItemAttributeType.ITEM_ATTRIBUTE_WEIGHT => ItemTypeAttribute.Weight,
            ItemAttributeType.ITEM_ATTRIBUTE_ATTACK => ItemTypeAttribute.Attack,
            ItemAttributeType.ITEM_ATTRIBUTE_DEFENSE => ItemTypeAttribute.Defense,
            ItemAttributeType.ITEM_ATTRIBUTE_EXTRADEFENSE => ItemTypeAttribute.ExtraDefense,
            ItemAttributeType.ITEM_ATTRIBUTE_ARMOR => ItemTypeAttribute.Armor,
            ItemAttributeType.ITEM_ATTRIBUTE_HITCHANCE => ItemTypeAttribute.HitChance,
            //ItemAttributeType.ITEM_ATTRIBUTE_SHOOTRANGE => ItemTypeAttribute.ShootRange,
            //ItemAttributeType.ITEM_ATTRIBUTE_OWNER => ItemTypeAttribute.Owner,
            //ItemAttributeType.ITEM_ATTRIBUTE_WRAPID => ItemTypeAttribute.WrapId,
            ItemAttributeType.ITEM_ATTRIBUTE_DURATION => ItemTypeAttribute.Duration,
            ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE => ItemTypeAttribute.DecayingState,
            //ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER => ItemTypeAttribute.CorpseOwner,
            ItemAttributeType.ITEM_ATTRIBUTE_CHARGES => ItemTypeAttribute.Charges,
            //ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE => ItemTypeAttribute.FluidType,
            ItemAttributeType.ITEM_ATTRIBUTE_DOORID => ItemTypeAttribute.HouseDoorId,
            ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO => ItemTypeAttribute.DecayTo,
            ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM => ItemTypeAttribute.StoreItem,
            ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED => ItemTypeAttribute.AttackSpeed,
            ItemAttributeType.ITEM_ATTRIBUTE_CUSTOM => ItemTypeAttribute.CustomAttributes,
            _ => ItemTypeAttribute.None
        };
    }

    public static ItemAttributeType ToItemAttributeType(this ItemAttribute value)
    {
        return value switch
        {
            ItemAttribute.ActionId => ItemAttributeType.ITEM_ATTRIBUTE_ACTIONID,
            ItemAttribute.UniqueId => ItemAttributeType.ITEM_ATTRIBUTE_UNIQUEID,
            ItemAttribute.Description => ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION,
            ItemAttribute.Text => ItemAttributeType.ITEM_ATTRIBUTE_TEXT,
            ItemAttribute.Date => ItemAttributeType.ITEM_ATTRIBUTE_DATE,
            ItemAttribute.Writer => ItemAttributeType.ITEM_ATTRIBUTE_WRITER,
            ItemAttribute.Name => ItemAttributeType.ITEM_ATTRIBUTE_NAME,
            ItemAttribute.Article => ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE,
            ItemAttribute.PluralName => ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME,
            ItemAttribute.Weight => ItemAttributeType.ITEM_ATTRIBUTE_WEIGHT,
            ItemAttribute.Attack => ItemAttributeType.ITEM_ATTRIBUTE_ATTACK,
            ItemAttribute.Defense => ItemAttributeType.ITEM_ATTRIBUTE_DEFENSE,
            ItemAttribute.ExtraDefense => ItemAttributeType.ITEM_ATTRIBUTE_EXTRADEFENSE,
            ItemAttribute.Armor => ItemAttributeType.ITEM_ATTRIBUTE_ARMOR,
            ItemAttribute.HitChance => ItemAttributeType.ITEM_ATTRIBUTE_HITCHANCE,
            ItemAttribute.ShootRange => ItemAttributeType.ITEM_ATTRIBUTE_SHOOTRANGE,
            ItemAttribute.Owner => ItemAttributeType.ITEM_ATTRIBUTE_OWNER,
            ItemAttribute.WrapId => ItemAttributeType.ITEM_ATTRIBUTE_WRAPID,
            ItemAttribute.Duration => ItemAttributeType.ITEM_ATTRIBUTE_DURATION,
            ItemAttribute.DecayState => ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE,
            ItemAttribute.CorpseOwner => ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER,
            ItemAttribute.Charges => ItemAttributeType.ITEM_ATTRIBUTE_CHARGES,
            ItemAttribute.FluidType => ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE,
            ItemAttribute.DoorId => ItemAttributeType.ITEM_ATTRIBUTE_DOORID,
            ItemAttribute.DecayTo => ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO,
            ItemAttribute.StoreItem => ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM,
            ItemAttribute.AttackSpeed => ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED,
            ItemAttribute.Custom => ItemAttributeType.ITEM_ATTRIBUTE_CUSTOM,
            _ => ItemAttributeType.ITEM_ATTRIBUTE_NONE
        };
    }

    public static bool IsAttributeInteger(this ItemAttributeType type)
    {
        return type switch
        {
            ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM or
                ItemAttributeType.ITEM_ATTRIBUTE_ACTIONID or
                ItemAttributeType.ITEM_ATTRIBUTE_UNIQUEID or
                ItemAttributeType.ITEM_ATTRIBUTE_DATE or
                ItemAttributeType.ITEM_ATTRIBUTE_WEIGHT or
                ItemAttributeType.ITEM_ATTRIBUTE_ATTACK or
                ItemAttributeType.ITEM_ATTRIBUTE_DEFENSE or
                ItemAttributeType.ITEM_ATTRIBUTE_EXTRADEFENSE or
                ItemAttributeType.ITEM_ATTRIBUTE_ARMOR or
                ItemAttributeType.ITEM_ATTRIBUTE_HITCHANCE or
                ItemAttributeType.ITEM_ATTRIBUTE_SHOOTRANGE or
                ItemAttributeType.ITEM_ATTRIBUTE_OWNER or
                ItemAttributeType.ITEM_ATTRIBUTE_DURATION or
                ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE or
                ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER or
                ItemAttributeType.ITEM_ATTRIBUTE_CHARGES or
                ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE or
                ItemAttributeType.ITEM_ATTRIBUTE_DOORID or
                ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO or
                ItemAttributeType.ITEM_ATTRIBUTE_WRAPID or
                ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED
                => true,
            _ => false
        };
    }

    public static bool IsAttributeString(this ItemAttributeType type)
    {
        return type switch
        {
            ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION or
                ItemAttributeType.ITEM_ATTRIBUTE_TEXT or
                ItemAttributeType.ITEM_ATTRIBUTE_WRITER or
                ItemAttributeType.ITEM_ATTRIBUTE_NAME or
                ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE or
                ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME
                => true,
            _ => false
        };
    }
}