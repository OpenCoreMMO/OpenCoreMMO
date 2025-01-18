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

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Group", "", LuaGroupCreate);
        RegisterMetaMethod(luaState, "Group", "__eq", LuaUserdataCompare<IGroup>);

        RegisterMethod(luaState, "Group", "getId", LuaGroupGetId);
        RegisterMethod(luaState, "Group", "getName", LuaGroupGetName);
        RegisterMethod(luaState, "Group", "getFlags", LuaGroupGetFlags);
        RegisterMethod(luaState, "Group", "getAccess", LuaGroupGetAccess);
        RegisterMethod(luaState, "Group", "getMaxDepotItems", LuaGroupGetMaxDepotItems);
        RegisterMethod(luaState, "Group", "getMaxVipEntries", LuaGroupGetMaxVipEntries);
        RegisterMethod(luaState, "Group", "hasFlag", LuaGroupHasFlag);
    }

    private static int LuaGroupCreate(LuaState luaState)
    {
        // Group(id)
        var id = GetNumber<byte>(luaState, 2);

        if (_groupStore.TryGetValue(id, out var group) && group is not null)
        {
            PushUserdata(luaState, group);
            SetMetatable(luaState, -1, "Group");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaGroupGetId(LuaState luaState)
    {
        // group:getId()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
            Lua.PushNumber(luaState, group.Id);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGroupGetName(LuaState luaState)
    {
        // group:getName()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
            Lua.PushString(luaState, group.Name);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGroupGetFlags(LuaState luaState)
    {
        // group:getFlags()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
        {
            var flags = new BitArray(Enum.GetValues(typeof(PlayerFlag)).Length);

            foreach (var flag in group.Flags)
                flags.Set((int)flag.Key, flag.Value); 

            Lua.PushNumber(luaState, flags.ToULong());
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaGroupGetAccess(LuaState luaState)
    {
        // group:getAccess()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
            Lua.PushBoolean(luaState, group.Access);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGroupGetMaxDepotItems(LuaState luaState)
    {
        // group:getMaxDepotItems()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
            Lua.PushNumber(luaState, group.MaxDepotItems);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGroupGetMaxVipEntries(LuaState luaState)
    {
        // group:getMaxVipEntries()
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null)
            Lua.PushNumber(luaState, group.MaxVipEntries);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    public static int LuaGroupHasFlag(LuaState luaState)
    {
        // group:hasFlag(flag)
        var group = GetUserdata<IGroup>(luaState, 1);
        if (group != null  && Lua.IsNumber(luaState, 2))
            Lua.PushBoolean(luaState, group.FlagIsEnabled(GetNumber<PlayerFlag>(luaState, 2)));
        else
            Lua.PushNil(luaState);

        return 1;
    }
}