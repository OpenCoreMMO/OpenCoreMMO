using NeoServer.Game.Common.Item;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Extensions
{
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
                ItemAttributeType.ITEM_ATTRIBUTE_DATE => ItemAttribute.WrittenDate,
                ItemAttributeType.ITEM_ATTRIBUTE_WRITER => ItemAttribute.WrittenBy,
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
                //todo: implement this
                //ItemAttributeType.ITEM_ATTRIBUTE_OWNER => ItemAttribute.Owner?,
                //ItemAttributeType.ITEM_ATTRIBUTE_WRAPID => ItemAttribute.WrapId?,
                ItemAttributeType.ITEM_ATTRIBUTE_DURATION => ItemAttribute.Duration,
                ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE => ItemAttribute.DecayingState,
                ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER => ItemAttribute.CorpseType,
                ItemAttributeType.ITEM_ATTRIBUTE_CHARGES => ItemAttribute.Charges,
                ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE => ItemAttribute.PoolLiquidType,
                ItemAttributeType.ITEM_ATTRIBUTE_DOORID => ItemAttribute.HouseDoorId,
                ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO => ItemAttribute.DecayTo,
                ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM => ItemAttribute.Item,
                ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED => ItemAttribute.Speed,
                ItemAttributeType.ITEM_ATTRIBUTE_CUSTOM => ItemAttribute.CustomAttributes,
                _ => ItemAttribute.None,
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
                ItemAttribute.WrittenDate => ItemAttributeType.ITEM_ATTRIBUTE_DATE,
                ItemAttribute.WrittenBy => ItemAttributeType.ITEM_ATTRIBUTE_WRITER,
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
                //todo: implement this
                //ItemAttribute.Owner? => ItemAttributeType.ITEM_ATTRIBUTE_OWNER,
                //ItemAttribute.WrapId? => ItemAttributeType.ITEM_ATTRIBUTE_WRAPID
                ItemAttribute.Duration => ItemAttributeType.ITEM_ATTRIBUTE_DURATION,
                ItemAttribute.DecayingState => ItemAttributeType.ITEM_ATTRIBUTE_DECAYSTATE,
                ItemAttribute.CorpseType => ItemAttributeType.ITEM_ATTRIBUTE_CORPSEOWNER,
                ItemAttribute.Charges => ItemAttributeType.ITEM_ATTRIBUTE_CHARGES,
                ItemAttribute.PoolLiquidType => ItemAttributeType.ITEM_ATTRIBUTE_FLUIDTYPE,
                ItemAttribute.HouseDoorId => ItemAttributeType.ITEM_ATTRIBUTE_DOORID,
                ItemAttribute.DecayTo => ItemAttributeType.ITEM_ATTRIBUTE_DECAYTO,
                ItemAttribute.Item => ItemAttributeType.ITEM_ATTRIBUTE_STOREITEM,
                ItemAttribute.Speed => ItemAttributeType.ITEM_ATTRIBUTE_ATTACK_SPEED,
                ItemAttribute.CustomAttributes => ItemAttributeType.ITEM_ATTRIBUTE_CUSTOM,
                _ => ItemAttributeType.ITEM_ATTRIBUTE_NONE,
            };
        }


    }
}
