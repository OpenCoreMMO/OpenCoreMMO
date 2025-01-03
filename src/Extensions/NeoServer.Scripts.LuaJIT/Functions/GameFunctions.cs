using LuaNET;
using NeoServer.Data.Entities;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GameFunctions : LuaScriptInterface, IGameFunctions
{
    private static IScripts _scripts;
    private static IItemTypeStore _itemTypeStore;
    private static IItemFactory _itemFactory;
    private static IMap _map;
    private static ICreatureFactory _creatureFactory;
    private static IGameCreatureManager _gameCreatureManager;
    private static ServerConfiguration _serverConfiguration;
    private static IStaticToDynamicTileService _staticToDynamicTileService;

    public GameFunctions(
        IScripts scripts,
        IItemTypeStore itemTypeStore,
        IItemFactory itemFactory,
        IMap map,
        ICreatureFactory creatureFactory,
        IGameCreatureManager gameCreatureManager,
        ServerConfiguration serverConfiguration,
        IStaticToDynamicTileService staticToDynamicTileService) : base(nameof(GameFunctions))
    {
        _scripts = scripts;
        _itemTypeStore = itemTypeStore;
        _itemFactory = itemFactory;
        _map = map;
        _creatureFactory = creatureFactory;
        _gameCreatureManager = gameCreatureManager;
        _serverConfiguration = serverConfiguration;
        _staticToDynamicTileService = staticToDynamicTileService;
    }

    public void Init(LuaState L)
    {
        RegisterTable(L, "Game");

        RegisterMethod(L, "Game", "getReturnMessage", LuaGameGetReturnMessage);

        RegisterMethod(L, "Game", "createItem", LuaGameCreateItem);
        RegisterMethod(L, "Game", "createMonster", LuaGameCreateMonster);
        RegisterMethod(L, "Game", "createNpc", LuaGameCreateNpc);

        RegisterMethod(L, "Game", "reload", LuaGameReload);

        RegisterMethod(L, "Game", "getPlayers", LuaGameGetPlayers);
    }

    private static int LuaGameGetReturnMessage(LuaState L)
    {
        // Game.getReturnMessage(value)
        var returnValue = GetNumber<ReturnValueType>(L, 1);
        PushString(L, returnValue.GetReturnMessage());
        return 1;
    }

    private static int LuaGameCreateItem(LuaState L)
    {
        // Game.createItem(itemId or name, count, position)

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

        var count = GetNumber(L, 2, 1);
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
        if (Lua.GetTop(L) >= 3) position = GetPosition(L, 3);

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
                if (!hasTable) Lua.PushNil(L);
                continue;
            }

            if (position.X != 0)
            {
                var tile = _map.GetTile(position);
                if (tile == null)
                {
                    if (!hasTable) Lua.PushNil(L);
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

                if (result)
                {
                    if (!hasTable) Lua.PushNil(L);
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

    private static int LuaGameCreateMonster(LuaState L)
    {
        // Game.createMonster(monsterName, position, extended = false, force = false, master = nil)
        //todo: implements force parameter

        var monsterName = GetString(L, 1);

        var position = GetPosition(L, 2);
        var extended = GetBoolean(L, 3, false);
        var force = GetBoolean(L, 4, false);

        ICreature master = null;

        var isSummon = false;
        if (Lua.GetTop(L) >= 5)
        {
            master = GetUserdata<ICreature>(L, 5);
            if (master.IsNotNull()) isSummon = true;
        }

        IMonster monster = null;
        if (isSummon && master != null)
            monster = _creatureFactory.CreateSummon(monsterName, master);
        else
            monster = _creatureFactory.CreateMonster(monsterName);

        if (!monster)
        {
            Lua.PushNil(L);
            return 1;
        }

        var tileToBorn = _map[position];

        if (tileToBorn is IDynamicTile { HasCreature: false })
        {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone))
            {
                Lua.PushNil(L);
                return 1;
            }

            if (isSummon)
            {
                monster.SetNewLocation(tileToBorn.Location);
                _map.PlaceCreature(monster);
            }
            else
            {
                monster.Born(position);
            }

            PushUserdata(L, monster);
            SetMetatable(L, -1, "Monster");

            return 1;
        }

        foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasCreature: false })
            {
                if (isSummon)
                {
                    monster.SetNewLocation(neighbour);
                    _map.PlaceCreature(monster);
                }
                else
                {
                    monster.Born(neighbour);
                }

                PushUserdata(L, monster);
                SetMetatable(L, -1, "Monster");

                return 1;
            }

        Lua.PushNil(L);
        return 1;
    }

    private static int LuaGameCreateNpc(LuaState L)
    {
        // Game.createNpc(npcName, position, extended = false, force = false)
        //todo: implements force parameter

        var ncpName = GetString(L, 1);

        var position = GetPosition(L, 2);
        var extended = GetBoolean(L, 3, false);
        var force = GetBoolean(L, 4, false);

        var npc = _creatureFactory.CreateNpc(ncpName);

        if (!npc)
        {
            Lua.PushNil(L);
            return 1;
        }

        var tileToBorn = _map[position];

        if (tileToBorn is IDynamicTile { HasCreature: false })
        {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone))
            {
                Lua.PushNil(L);
                return 1;
            }

            npc.SetNewLocation(tileToBorn.Location);
            _map.PlaceCreature(npc);

            PushUserdata(L, npc);
            SetMetatable(L, -1, "Npc");

            return 1;
        }

        foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasCreature: false })
            {
                npc.SetNewLocation(neighbour);
                _map.PlaceCreature(npc);

                PushUserdata(L, npc);
                SetMetatable(L, -1, "Npc");

                return 1;
            }

        Lua.PushNil(L);
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
                        _scripts.LoadScripts($"{dir}/scripts/libs", true, true);

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

    private static int LuaGameGetPlayers(LuaState L)
    {
        // Game.getPlayers()
        var allPlayers = _gameCreatureManager.GetAllLoggedPlayers();

        Lua.CreateTable(L, allPlayers.Count(), 0);

        int index = 0;
        foreach (var player in allPlayers) {
            PushUserdata(L, player);
            SetMetatable(L, -1, "Player");
            Lua.RawSetI(L, -2, ++index);
        }
        return 1;
    }
}