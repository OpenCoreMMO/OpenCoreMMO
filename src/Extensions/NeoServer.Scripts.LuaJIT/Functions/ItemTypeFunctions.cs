using LuaNET;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Domain.Extensions;
using NeoServer.Domain.Items.Bases;
using System;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ItemTypeFunctions : LuaScriptInterface, IItemTypeFunctions
{
    private static IItemTypeStore _itemTypeStore;

    public ItemTypeFunctions(IItemTypeStore itemTypeStore) : base(nameof(ItemTypeFunctions))
    {
        _itemTypeStore = itemTypeStore;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "ItemType", "", LuaCreateItemType);
        RegisterMetaMethod(luaState, "ItemType", "__eq", LuaUserdataCompare<IItemType>);

        RegisterMethod(luaState, "ItemType", "isCorpse", LuaItemTypeIsCorpse);
        RegisterMethod(luaState, "ItemType", "isMovable", LuaItemTypeIsMoveable);
        RegisterMethod(luaState, "ItemType", "isStackable", LuaItemTypeIsStackable);
        RegisterMethod(luaState, "ItemType", "isFluidContainer", LuaItemTypeIsFluidContainer);
        RegisterMethod(luaState, "ItemType", "isKey", LuaItemTypeIsKey);

        RegisterMethod(luaState, "ItemType", "getType", LuaItemTypeGetType);
        RegisterMethod(luaState, "ItemType", "getId", LuaItemTypeGetId);
        RegisterMethod(luaState, "ItemType", "getName", LuaItemTypeGetName);

        RegisterMethod(luaState, "ItemType", "getWeight", LuaItemTypeGetWeight);

        RegisterMethod(luaState, "ItemType", "getDestroyId", LuaItemDestroyId);

        RegisterMethod(luaState, "ItemType", "hasAttribute", LuaItemHasAttribute);
        RegisterMethod(luaState, "ItemType", "getAttribute", LuaItemGetAttribute);
    }

    public static int LuaCreateItemType(LuaState luaState)
    {
        // ItemType(id or name)
        ushort id = 0;
        IItemType itemType = null;
        if (Lua.IsNumber(luaState, 2))
        {
            id = GetNumber<ushort>(luaState, 2);
            itemType = _itemTypeStore.Get(id);
        }
        else
        {
            var name = GetString(luaState, 2);
            itemType = _itemTypeStore.GetByName(name);
        }

        PushUserdata(luaState, itemType);
        SetMetatable(luaState, -1, "ItemType");

        return 1;
    }

    public static int LuaItemTypeIsCorpse(LuaState luaState)
    {
        // itemType:isCorpse()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsCorpse());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsMoveable(LuaState luaState)
    {
        // itemType:isMovable()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsMovable());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsStackable(LuaState luaState)
    {
        // itemType:isStackable()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsStackable());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsFluidContainer(LuaState luaState)
    {
        // itemType:isFluidContainer()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsFluidContainer());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeIsKey(LuaState luaState)
    {
        // itemType:isKey()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushBoolean(luaState, itemType.IsKey());
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetType(LuaState luaState)
    {
        // itemType:getType()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.ServerId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetId(LuaState luaState)
    {
        // itemType:getId()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.ServerId);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetName(LuaState luaState)
    {
        // itemType:getName()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushString(luaState, itemType.Name);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemTypeGetWeight(LuaState luaState)
    {
        // itemType:getWeight(count = 1)
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
        {
            var count = GetNumber<ushort>(luaState, 2, 1);
            var weight = itemType.Weight * float.Max(1, count);
            Lua.PushNumber(luaState, weight);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaItemDestroyId(LuaState luaState)
    {
        // itemType:getDestroyId()
        var itemType = GetUserdata<IItemType>(luaState, 1);
        if (itemType != null)
            Lua.PushNumber(luaState, itemType.DestroyTo);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaItemHasAttribute(LuaState luaState)
    {
        // item:hasAttribute(key)
        var itemType = GetUserdata<IItemType>(luaState, 1);

        if (itemType == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attribute = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attribute = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attribute = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        var hasAttribute = false;

        if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_NAME)
            hasAttribute = true;
        else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME)
            hasAttribute = true;
        else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE)
            hasAttribute = true;
        else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION)
            hasAttribute = true;
        else
            hasAttribute = itemType.Attributes.HasAttribute(attribute.ToItemAttribute());
        
        Lua.PushBoolean(luaState, hasAttribute);

        return 1;
    }

    public static int LuaItemGetAttribute(LuaState luaState)
    {
        // item:getAttribute(key)
        var itemType = GetUserdata<IItemType>(luaState, 1);

        if (itemType == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var attribute = ItemAttributeType.ITEM_ATTRIBUTE_NONE;
        if (Lua.IsNumber(luaState, 2))
            attribute = GetNumber<ItemAttributeType>(luaState, 2);
        else if (Lua.IsString(luaState, 2))
            attribute = EnumExtensions.FromDescription<ItemAttributeType>(GetString(luaState, 2));

        if (attribute.IsAttributeInteger())
        {
            var attributeValue = itemType.Attributes.GetAttribute<long>(attribute.ToItemAttribute());
            Lua.PushNumber(luaState, attributeValue);
        }
        else if (attribute.IsAttributeString())
        {
            var attributeValue = string.Empty;

            if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_NAME)
                attributeValue = itemType.Name;
            else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_PLURALNAME)
                attributeValue = itemType.PluralName;
            else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_ARTICLE)
                attributeValue = itemType.Article;
            else if (attribute == ItemAttributeType.ITEM_ATTRIBUTE_DESCRIPTION)
                attributeValue = itemType.Description;
            else
                attributeValue = itemType.Attributes.GetAttribute(attribute.ToItemAttribute());
            
            Lua.PushString(luaState, attributeValue);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}