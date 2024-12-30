using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using System.Collections;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GroupFunctions : LuaScriptInterface, IGroupFunctions
{
    private static IGroupStore _groupStore;

    public GroupFunctions(IGroupStore groupStore) : base(nameof(GroupFunctions))
    {
        _groupStore = groupStore;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Group", "", LuaGroupCreate);
        RegisterMetaMethod(L, "Group", "__eq", LuaUserdataCompare<IGroup>);

        RegisterMethod(L, "Group", "getId", LuaGroupGetId);
        RegisterMethod(L, "Group", "getName", LuaGroupGetName);
        RegisterMethod(L, "Group", "getFlags", LuaGroupGetFlags);
        RegisterMethod(L, "Group", "getAccess", LuaGroupGetAccess);
        RegisterMethod(L, "Group", "getMaxDepotItems", LuaGroupGetMaxDepotItems);
        RegisterMethod(L, "Group", "getMaxVipEntries", LuaGroupGetMaxVipEntries);
        RegisterMethod(L, "Group", "hasFlag", LuaGroupHasFlag);
    }

    private static int LuaGroupCreate(LuaState L)
    {
        // Group(id)
        var id = GetNumber<byte>(L, 2);

        if (_groupStore.TryGetValue(id, out var group) && group is not null)
        {
            PushUserdata(L, group);
            SetMetatable(L, -1, "Group");
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaGroupGetId(LuaState L)
    {
        // group:getId()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
            Lua.PushNumber(L, group.Id);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGroupGetName(LuaState L)
    {
        // group:getName()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
            Lua.PushString(L, group.Name);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGroupGetFlags(LuaState L)
    {
        // group:getFlags()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
        {
            var flags = new BitArray(Enum.GetValues(typeof(PlayerFlag)).Length);

            foreach (var flag in group.Flags)
                flags.Set((int)flag.Key, flag.Value); 

            Lua.PushNumber(L, flags.ToULong());
        }
        else
        {
            Lua.PushNil(L);
        }

        return 1;
    }

    public static int LuaGroupGetAccess(LuaState L)
    {
        // group:getAccess()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
            Lua.PushNumber(L, group.Access);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGroupGetMaxDepotItems(LuaState L)
    {
        // group:getMaxDepotItems()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
            Lua.PushNumber(L, group.MaxDepotItems);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGroupGetMaxVipEntries(LuaState L)
    {
        // group:getMaxVipEntries()
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null)
            Lua.PushNumber(L, group.MaxVipEntries);
        else
            Lua.PushNil(L);

        return 1;
    }

    public static int LuaGroupHasFlag(LuaState L)
    {
        // group:hasFlag(flag)
        var group = GetUserdata<IGroup>(L, 1);
        if (group != null  && Lua.IsNumber(L, 2))
            Lua.PushBoolean(L, group.Flags.ContainsKey(GetNumber<PlayerFlag>(L, 2)));
        else
            Lua.PushNil(L);

        return 1;
    }
}