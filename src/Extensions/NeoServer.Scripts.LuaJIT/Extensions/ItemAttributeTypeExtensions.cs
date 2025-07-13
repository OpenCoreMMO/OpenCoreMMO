using NeoServer.Domain.Common.Item;
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
}