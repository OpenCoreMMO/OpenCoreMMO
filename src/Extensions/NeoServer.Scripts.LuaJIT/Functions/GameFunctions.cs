using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GameFunctions : LuaScriptInterface, IGameFunctions
{
    private static IScripts _scripts;
    private static IItemTypeStore _itemTypeStore;
    private static IItemFactory _itemFactory;
    private static ServerConfiguration _serverConfiguration;

    public GameFunctions(
        IScripts scripts,
        IItemTypeStore itemTypeStore,
        IItemFactory itemFactory,
        ServerConfiguration serverConfiguration) : base(nameof(GameFunctions))
    {
        _scripts = scripts;
        _itemTypeStore = itemTypeStore;
        _itemFactory = itemFactory;
        _serverConfiguration = serverConfiguration;
    }

    public void Init(LuaState L)
    {
        RegisterTable(L, "Game");
        RegisterMethod(L, "Game", "getReturnMessage", LuaGameGetReturnMessage);
        RegisterMethod(L, "Game", "reload", LuaGameReload);
        RegisterMethod(L, "Game", "createItem", LuaGameCreateItem);
    }

    private static int LuaGameGetReturnMessage(LuaState L)
    {
        // Game.getReturnMessage(value)
        var returnValue = GetNumber<ReturnValueType>(L, 1);
        PushString(L, returnValue.GetReturnMessage());
        return 1;
    }

    private static int LuaGameReload(LuaState L)
    {
        // Game.reload(reloadType)
        var reloadType = GetNumber<ReloadType>(L, 1);
        if (reloadType == ReloadType.RELOAD_TYPE_NONE)
        {
            ReportError(nameof(LuaGameReload), "Reload type is none");
            PushBoolean(L, false);
            return 0;
        }

        if (reloadType >= ReloadType.RELOAD_TYPE_LAST)
        {
            ReportError(nameof(LuaGameReload), "Reload type not exist");
            PushBoolean(L, false);
            return 0;
        }
        try
        {
            switch (reloadType)
            {
                case ReloadType.RELOAD_TYPE_SCRIPTS:
                    {
                        var dir = AppContext.BaseDirectory + _serverConfiguration.DataLuaJit;
                        _scripts.ClearAllScripts();
                        _scripts.LoadScripts($"{dir}/scripts", false, true);

                        Lua.GC(LuaEnvironment.GetInstance().GetLuaState(), LuaGCParam.Collect, 0);
                    }

                    break;
                default:
                    ReportError(nameof(LuaGameReload), "Reload type not implemented");
                    break;
            }
        }
        catch (Exception e)
        {
            PushBoolean(L, false);
            return 0;
        }

        PushBoolean(L, true);
        return 1;
    }

    private static int LuaGameCreateItem(LuaState L)
    {
        // Game.createItem(itemId or name[, count[, position]])

        ushort itemId;
        if (Lua.IsNumber(L, 1))
        {
            itemId = GetNumber<ushort>(L, 1);
        }
        else
        {
            var itemName = GetString(L, 1);

            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null)
            {
                Lua.PushNil(L);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber<int>(L, 2, 1);
        var itemCount = 1;
        var subType = 1;

        var it = _itemTypeStore.Get(itemId);
        if (it.HasSubType())
        {
            if (it.IsStackable())
            {
                itemCount = (int)Math.Ceiling(count / (float)it.Count);
            }

            subType = count;
        }
        else
        {
            itemCount = int.Max(1, count);
        }

        var position = new Location();
        if (Lua.GetTop(L) >= 3)
        {
            position = GetPosition(L, 3);
        }

        var hasTable = itemCount > 1;
        if (hasTable)
        {
            Lua.NewTable(L);
        }
        else if (itemCount == 0)
        {
            Lua.PushNil(L);
            return 1;
        }

        var map = IoC.GetInstance<IMap>();

        for (int i = 1; i <= itemCount; ++i)
        {
            var stackCount = subType;
            if (it.IsStackable())
            {
                stackCount = int.Max(stackCount, it.Count);
                subType -= stackCount;
            }

            var item = _itemFactory.Create(itemId, position, stackCount);
            if (item == null)
            {
                if (!hasTable)
                {
                    Lua.PushNil(L);
                }
                return 1;
            }

            if (position.X != 0)
            {
                var tile = map.GetTile(position);
                if (tile == null)
                {
                    if (!hasTable)
                    {
                        Lua.PushNil(L);
                    }
                    return 1;
                }

                var result = ((IDynamicTile)tile).AddItem(item);

                if (result.Succeeded)
                {
                    if (!hasTable)
                    {
                        Lua.PushNil(L);
                    }
                    return 1;
                }
            }
            else
            {
                GetScriptEnv().AddTempItem(item);
                //todo: check if need this
                //item->setParent(VirtualCylinder::virtualCylinder);
            }

            if (hasTable)
            {
                Lua.PushNumber(L, i);
                PushUserdata(L, item);
                SetItemMetatable(L, -1, item);
                Lua.SetTable(L, -3);
            }
            else
            {
                PushUserdata(L, item);
                SetItemMetatable(L, -1, item);
            }
        }

        return 1;
    }
}
