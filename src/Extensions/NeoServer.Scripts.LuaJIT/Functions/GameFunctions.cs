using LuaNET;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GameFunctions : LuaScriptInterface, IGameFunctions
{
    private static IItemTypeStore _itemTypeStore;
    private static IItemFactory _itemFactory;
    private static IMap _map;
    private static ICreatureFactory _creatureFactory;
    private static IGameCreatureManager _gameCreatureManager;
    private static IStaticToDynamicTileService _staticToDynamicTileService;
    private static IReloadManager _reloadManager;

    public GameFunctions(
        IItemTypeStore itemTypeStore,
        IItemFactory itemFactory,
        IMap map,
        ICreatureFactory creatureFactory,
        IGameCreatureManager gameCreatureManager,
        ServerConfiguration serverConfiguration,
        IStaticToDynamicTileService staticToDynamicTileService,
        IReloadManager reloadManager) : base(nameof(GameFunctions))
    {
        _itemTypeStore = itemTypeStore;
        _itemFactory = itemFactory;
        _map = map;
        _creatureFactory = creatureFactory;
        _gameCreatureManager = gameCreatureManager;
        _staticToDynamicTileService = staticToDynamicTileService;
        _reloadManager = reloadManager;
    }

    public void Init(LuaState luaState)
    {
        RegisterTable(luaState, "Game");

        RegisterMethod(luaState, "Game", "createNpcType", NpcTypeFunctions.LuaNpcTypeCreate);

        RegisterMethod(luaState, "Game", "getReturnMessage", LuaGameGetReturnMessage);

        RegisterMethod(luaState, "Game", "createItem", LuaGameCreateItem);
        RegisterMethod(luaState, "Game", "createMonster", LuaGameCreateMonster);
        RegisterMethod(luaState, "Game", "createNpc", LuaGameCreateNpc);

        RegisterMethod(luaState, "Game", "reload", LuaGameReload);

        RegisterMethod(luaState, "Game", "getPlayers", LuaGameGetPlayers);
        RegisterMethod(luaState, "Game", "getNormalizedPlayerName", LuaGameGetNormalizedPlayerNameFunction);

        RegisterMethod(luaState, "Game", "getEventCallbacks", LuaGameGetEventCallbacks);
    }

    private static int LuaGameGetEventCallbacks(LuaState luaState)
    {
        // Game.getEventCallbacks()
        Lua.NewTable(luaState); // create a new table

        // Push the EventCallbackFunctions.LuaEventCallbackLoad function
        Lua.PushCFunction(luaState, EventCallbackFunctions.LuaEventCallbackLoad);

        // Register all EventCallbackType enum entries except None
        foreach (EventCallbackType value in Enum.GetValues(typeof(EventCallbackType)))
        {
            if (value == EventCallbackType.None)
                continue;

            // Make the first letter lowercase
            var methodName = value.ToString();
            if (!string.IsNullOrEmpty(methodName))
                methodName = char.ToLowerInvariant(methodName[0]) + methodName.Substring(1);

            Lua.PushString(luaState, methodName);
            Lua.PushValue(luaState, -2); // copy the function reference to the top of the stack
            Lua.SetTable(luaState, -4); // set table[methodName] = function
        }

        Lua.Pop(luaState, 1); // pop the function
        return 1;
    }

    private int LuaGameGetNormalizedPlayerNameFunction(LuaState luaState)
    {
        // Game.getNormalizedPlayerName(name[, isNewName = false])
        var name = GetString(luaState, 1);

        //todo: implement isNewName logic
        var isNewName = GetBoolean(luaState, 2, false);

        _gameCreatureManager.TryGetPlayer(name, out var player);
        if (player != null)
            Lua.PushString(luaState, player.Name);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaGameGetReturnMessage(LuaState luaState)
    {
        // Game.getReturnMessage(value)
        var returnValue = GetNumber<ReturnValueType>(luaState, 1);
        PushString(luaState, returnValue.GetReturnMessage());
        return 1;
    }

    private static int LuaGameCreateItem(LuaState luaState)
    {
        // Game.createItem(itemId or name, count, position)

        ushort itemId;
        if (Lua.IsNumber(luaState, 1))
        {
            itemId = GetNumber<ushort>(luaState, 1);
        }
        else
        {
            var itemName = GetString(luaState, 1);

            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(luaState, 2, 1);
        var itemCount = 1;
        var subType = 1;

        var it = _itemTypeStore.Get(itemId);
        if (it.HasSubType())
        {
            if (it.IsStackable()) itemCount = (int)Math.Ceiling(count / (float)it.Count);

            subType = count;
        }
        else
        {
            itemCount = int.Max(1, count);
        }

        var position = new Location();
        if (Lua.GetTop(luaState) >= 3) position = GetPosition(luaState, 3);

        var hasTable = itemCount > 1;
        if (hasTable)
        {
            Lua.NewTable(luaState);
        }
        else if (itemCount == 0)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        for (var i = 1; i <= itemCount; ++i)
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
                if (!hasTable) Lua.PushNil(luaState);
                continue;
            }

            if (position.X != 0)
            {
                var tile = _map.GetTile(position);
                if (tile == null)
                {
                    if (!hasTable) Lua.PushNil(luaState);
                    continue;
                }

                if (tile is IStaticTile)
                {
                    tile = tile is IStaticTile staticTile ? staticTile.CreateClone(position) : tile;
                    tile = _staticToDynamicTileService.TransformIntoDynamicTile(tile);
                }

                var result = false;

                if (tile is IDynamicTile dynamicTile)
                    result = dynamicTile.AddItem(item).Succeeded;

                if (!result)
                {
                    if (!hasTable) Lua.PushNil(luaState);
                    continue;
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
                Lua.PushNumber(luaState, i);
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
                Lua.SetTable(luaState, -3);
            }
            else
            {
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
            }
        }

        return 1;
    }

    private static int LuaGameCreateMonster(LuaState luaState)
    {
        // Game.createMonster(monsterName, position, extended = false, force = false, master = nil)
        //todo: implements force parameter

        var monsterName = GetString(luaState, 1);

        var position = GetPosition(luaState, 2);
        var extended = GetBoolean(luaState, 3, false);
        var force = GetBoolean(luaState, 4, false);

        ICreature master = null;

        var isSummon = false;
        if (Lua.GetTop(luaState) >= 5)
        {
            master = GetUserdata<ICreature>(luaState, 5);
            if (master.IsNotNull()) isSummon = true;
        }

        IMonster monster = null;
        if (isSummon && master != null)
            monster = _creatureFactory.CreateSummon(monsterName, master);
        else
            monster = _creatureFactory.CreateMonster(monsterName);

        if (!monster)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var tileToBorn = _map[position];

        if (tileToBorn is IDynamicTile { HasAnyCreature: false } dynamicTile && dynamicTile.CanEnter(monster)
                                                                             && MonsterEnterTileRule.Rule.CanEnter(
                                                                                 tileToBorn, monster))
        {
            if (dynamicTile.ProtectionZone)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            monster.Born(position);

            PushUserdata(luaState, monster);
            SetMetatable(luaState, -1, "Monster");

            return 1;
        }

        foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasAnyCreature: false } neighbourTile &&
                neighbourTile.CanEnter(monster) && MonsterEnterTileRule.Rule.CanEnter(neighbourTile, monster))
            {
                monster.Born(neighbour);

                PushUserdata(luaState, monster);
                SetMetatable(luaState, -1, "Monster");

                return 1;
            }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaGameCreateNpc(LuaState luaState)
    {
        // Game.createNpc(npcName, position, extended = false, force = false)
        //todo: implements force parameter

        var ncpName = GetString(luaState, 1);

        var position = GetPosition(luaState, 2);
        var extended = GetBoolean(luaState, 3, false);
        var force = GetBoolean(luaState, 4, false);

        var npc = _creatureFactory.CreateNpc(ncpName);

        if (!npc)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var tileToBorn = _map[position];

        if (tileToBorn is IDynamicTile { HasAnyCreature: false } dynamicTile && dynamicTile.CanEnter(npc))
        {
            if (dynamicTile.HasFlag(TileFlags.ProtectionZone))
            {
                Lua.PushNil(luaState);
                return 1;
            }

            npc.SetNewLocation(dynamicTile.Location);
            _map.PlaceCreature(npc);

            PushUserdata(luaState, npc);
            SetMetatable(luaState, -1, "Npc");

            return 1;
        }

        foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasAnyCreature: false } neighbourTile && neighbourTile.CanEnter(npc))
            {
                npc.SetNewLocation(neighbour);
                _map.PlaceCreature(npc);

                PushUserdata(luaState, npc);
                SetMetatable(luaState, -1, "Npc");

                return 1;
            }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaGameReload(LuaState luaState)
    {
        // Game.reload(reloadType)
        var reloadType = GetNumber<ReloadType>(luaState, 1);
        var result = _reloadManager.Reload(reloadType);

        if (!result)
            ReportError(nameof(LuaGameReload), "Reload type not implemented");

        PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGameGetPlayers(LuaState luaState)
    {
        // Game.getPlayers()
        var allPlayers = _gameCreatureManager.GetAllLoggedPlayers();

        Lua.CreateTable(luaState, allPlayers.Count(), 0);

        var index = 0;
        foreach (var player in allPlayers)
        {
            PushUserdata(luaState, player);
            SetMetatable(luaState, -1, "Player");
            Lua.RawSetI(luaState, -2, ++index);
        }

        return 1;
    }
}