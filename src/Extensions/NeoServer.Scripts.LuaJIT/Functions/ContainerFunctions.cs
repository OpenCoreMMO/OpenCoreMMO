using System.Text;
using LuaNET;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ContainerFunctions : LuaScriptInterface, IContainerFunctions
{
    private static IItemTypeStore _itemTypeStore;
    private static IItemFactory _itemFactory;

    public ContainerFunctions(
        IItemTypeStore itemTypeStore,
        IItemFactory itemFactory
    ) : base(nameof(ContainerFunctions))
    {
        _itemTypeStore = itemTypeStore;
        _itemFactory = itemFactory;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Container", "Item", LuaContainerCreate);
        RegisterMetaMethod(luaState, "Container", "__eq", LuaUserdataCompare<IContainer>);

        RegisterMethod(luaState, "Container", "getSize", LuaContainerGetSize);
        RegisterMethod(luaState, "Container", "getCapacity", LuaContainerGetCapacity);
        RegisterMethod(luaState, "Container", "getEmptySlots", LuaContainerGetEmptySlots);
        RegisterMethod(luaState, "Container", "getContentDescription", LuaContainerGetContentDescription);
        RegisterMethod(luaState, "Container", "getItems", LuaContainerGetItems);
        RegisterMethod(luaState, "Container", "getItemHoldingCount", LuaContainerGetItemHoldingCount);
        RegisterMethod(luaState, "Container", "getItemCountById", LuaContainerGetItemCountById);

        RegisterMethod(luaState, "Container", "getItem", LuaContainerGetItem);
        RegisterMethod(luaState, "Container", "hasItem", LuaContainerHasItem);
        RegisterMethod(luaState, "Container", "addItem", LuaContainerAddItem);
        RegisterMethod(luaState, "Container", "addItemEx", LuaContainerAddItemEx);
        RegisterMethod(luaState, "Container", "getCorpseOwner", LuaContainerGetCorpseOwner);
    }

    private static int LuaContainerCreate(LuaState luaState)
    {
        // Container(uid)
        var id = GetNumber<uint>(luaState, 2);

        var container = GetScriptEnv().GetContainerByUID(id);
        if (container != null)
        {
            PushUserdata(luaState, container);
            SetMetatable(luaState, -1, "Container");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaContainerGetSize(LuaState luaState)
    {
        // container:getSize()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
            Lua.PushNumber(luaState, container.Items.Count);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaContainerGetCapacity(LuaState luaState)
    {
        // container:getCapacity()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
            Lua.PushNumber(luaState, container.Capacity);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaContainerGetEmptySlots(LuaState luaState)
    {
        // container:getEmptySlots([recursive = false])
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var slots = container.Capacity - container.Items.Count;
        var recursive = GetBoolean(luaState, 2, false);

        if (recursive)
            foreach (var item in container.Items)
                if (item is IContainer innerContainer)
                    slots += innerContainer.Capacity - innerContainer.Items.Count;

        Lua.PushNumber(luaState, slots);
        return 1;
    }

    public static int LuaContainerGetContentDescription(LuaState luaState)
    {
        // container:getContentDescription()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
        {
            var sb = new StringBuilder();
            foreach (var item in container.Items)
            {
                if (item is IContainer)
                    continue;

                sb.Append(item.Metadata.FullName);
                sb.Append(", ");
            }

            if (sb.Length > 2)
                sb.Length -= 2;

            Lua.PushString(luaState, sb.ToString());
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaContainerGetItems(LuaState luaState)
    {
        // container:getItems([recursive = false])
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var recursive = GetBoolean(luaState, 2, false);
        var items = recursive && container is not null && container.RecursiveItems != null
            ? container.RecursiveItems
            : container.Items;

        Lua.CreateTable(luaState, items.Count, 0);
        var index = 1;
        foreach (var item in items)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
            Lua.RawSetI(luaState, -2, index++);
        }

        return 1;
    }

    public static int LuaContainerGetItemHoldingCount(LuaState luaState)
    {
        // container:getItemHoldingCount()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
            Lua.PushNumber(luaState, container.Items.Count);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaContainerGetItemCountById(LuaState luaState)
    {
        // container:getItemCountById(itemId[, subType = -1])
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var itemId = 0;

        if (IsNumber(luaState, 2))
        {
            itemId = GetNumber<int>(luaState, 2);
        }
        else
        {
            var itemType = _itemTypeStore.GetByName(GetString(luaState, 2));
            if (itemType != null)
            {
                itemId = itemType.ServerId;
            }
            else
            {
                Lua.PushNil(luaState);
                return 1;
            }
        }

        var subType = GetNumber(luaState, 3, -1);

        var count = 0;
        foreach (var item in container.Items)
            if (item.Metadata.ServerId == itemId && (subType == -1 || item.GetSubType() == subType))
                count++;

        Lua.PushNumber(luaState, count);
        return 1;
    }

    public static int LuaContainerGetItem(LuaState luaState)
    {
        // container:getItem(index)
        var container = GetUserdata<IContainer>(luaState, 1);

        if (!container)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var index = GetNumber<int>(luaState, 2);
        var item = container.Items.ElementAtOrDefault(index);

        if (item != null)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaContainerHasItem(LuaState luaState)
    {
        // container:hasItem(item)
        var container = GetUserdata<IContainer>(luaState, 1);
        var item = GetUserdata<IItem>(luaState, 2);

        if (container != null)
            Lua.PushBoolean(luaState, container.Items.Contains(item));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaContainerAddItem(LuaState luaState)
    {
        // container:addItem(itemId[, count/subType = 1[, index = -1[, flags = 0]]])
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        ushort itemId = 0;

        if (IsNumber(luaState, 2))
        {
            itemId = GetNumber<ushort>(luaState, 2);
        }
        else
        {
            var itemType = _itemTypeStore.GetByName(GetString(luaState, 2));
            if (itemType != null)
            {
                itemId = itemType.ServerId;
            }
            else
            {
                Lua.PushNil(luaState);
                return 1;
            }
        }

        var count = GetNumber(luaState, 3, 1);
        var index = GetNumber(luaState, 4, -1);

        //todo: implement flags
        //var flags = GetNumber(luaState, 5, 0);

        var item = _itemFactory.Create(itemId, Location.Zero, count);
        if (item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var result = container.AddItem(item, index == -1);
        if (result.Succeeded)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
        }
        else
        {
            Lua.PushBoolean(luaState, false);
        }

        return 1;
    }

    public static int LuaContainerAddItemEx(LuaState luaState)
    {
        // container:addItemEx(item[, index = -1[, flags = 0]])
        var container = GetUserdata<IContainer>(luaState, 1);
        var item = GetUserdata<IItem>(luaState, 2);

        if (container == null || item == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var index = GetNumber(luaState, 3, -1);
        //todo: impelment flags
        //var flags = GetNumber(luaState, 4, 0);

        var result = container.AddItem(item, index == -1);
        Lua.PushBoolean(luaState, result.Succeeded);
        return 1;
    }

    public static int LuaContainerGetCorpseOwner(LuaState luaState)
    {
        // container:getCorpseOwner()
        var container = GetUserdata<IContainer>(luaState, 1);
        if (container != null)
            Lua.PushNumber(luaState, container.Attributes.GetAttribute<uint>(ItemAttribute.CorpseOwner));
        else
            Lua.PushNil(luaState);

        return 1;
    }
}