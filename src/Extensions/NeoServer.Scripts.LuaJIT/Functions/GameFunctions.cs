using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.UseCase.Monster;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Extensions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GameFunctions : LuaScriptInterface, IGameFunctions {
    private static ILuaEnvironment _luaEnvironment;
    private static IScripts _scripts;
    private static IItemTypeStore _itemTypeStore;
    private static IItemFactory _itemFactory;
    private static IMap _map;
    private static ICreatureFactory _creatureFactory;
    private static IGameCreatureManager _gameCreatureManager;
    private static ServerConfiguration _serverConfiguration;
    private static IStaticToDynamicTileService _staticToDynamicTileService;
    private static ICreateMonsterOrSummonUseCase _createMonsterOrSummonUseCase;

    public GameFunctions(
        ILuaEnvironment luaEnvironment,
        IScripts scripts,
        IItemTypeStore itemTypeStore,
        IItemFactory itemFactory,
        IMap map,
        ICreatureFactory creatureFactory,
        IGameCreatureManager gameCreatureManager,
        ServerConfiguration serverConfiguration,
        IStaticToDynamicTileService staticToDynamicTileService,
        ICreateMonsterOrSummonUseCase createMonsterOrSummonUseCase
        ) : base(nameof(GameFunctions)) {
        _luaEnvironment = luaEnvironment;
        _scripts = scripts;
        _itemTypeStore = itemTypeStore;
        _itemFactory = itemFactory;
        _map = map;
        _creatureFactory = creatureFactory;
        _gameCreatureManager = gameCreatureManager;
        _serverConfiguration = serverConfiguration;
        _staticToDynamicTileService = staticToDynamicTileService;
        _createMonsterOrSummonUseCase = createMonsterOrSummonUseCase;
    }

    public void Init(LuaState luaState) {
        RegisterTable(luaState, "Game");

        RegisterMethod(luaState, "Game", "getReturnMessage", LuaGameGetReturnMessage);

        RegisterMethod(luaState, "Game", "createItem", LuaGameCreateItem);
        RegisterMethod(luaState, "Game", "createMonster", LuaGameCreateMonster);
        RegisterMethod(luaState, "Game", "createNpc", LuaGameCreateNpc);

        RegisterMethod(luaState, "Game", "reload", LuaGameReload);

        RegisterMethod(luaState, "Game", "getPlayers", LuaGameGetPlayers);
    }

    private static int LuaGameGetReturnMessage(LuaState luaState) {
        // Game.getReturnMessage(value)
        var returnValue = GetNumber<ReturnValueType>(luaState, 1);
        PushString(luaState, returnValue.GetReturnMessage());
        return 1;
    }

    private static int LuaGameCreateItem(LuaState luaState) {
        // Game.createItem(itemId or name, count, position)

        ushort itemId;
        if (Lua.IsNumber(luaState, 1)) {
            itemId = GetNumber<ushort>(luaState, 1);
        } else {
            var itemName = GetString(luaState, 1);

            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null) {
                Lua.PushNil(luaState);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(luaState, 2, 1);
        var itemCount = 1;
        var subType = 1;

        var it = _itemTypeStore.Get(itemId);
        if (it.HasSubType()) {
            if (it.IsStackable()) itemCount = (int)Math.Ceiling(count / (float)it.Count);

            subType = count;
        } else {
            itemCount = int.Max(1, count);
        }

        var position = new Location();
        if (Lua.GetTop(luaState) >= 3) position = GetPosition(luaState, 3);

        var hasTable = itemCount > 1;
        if (hasTable) {
            Lua.NewTable(luaState);
        } else if (itemCount == 0) {
            Lua.PushNil(luaState);
            return 1;
        }

        for (int i = 1; i <= itemCount; ++i) {
            var stackCount = subType;
            if (it.IsStackable()) {
                stackCount = int.Max(stackCount, it.Count);
                subType -= stackCount;
            }

            var item = _itemFactory.Create(itemId, position, stackCount);
            if (item == null) {
                if (!hasTable) Lua.PushNil(luaState);
                continue;
            }

            if (position.X != 0) {
                var tile = _map.GetTile(position);
                if (tile == null) {
                    if (!hasTable) Lua.PushNil(luaState);
                    continue;
                }

                if (tile is IStaticTile) {
                    tile = tile is IStaticTile staticTile ? staticTile.CreateClone(position) : tile;
                    tile = _staticToDynamicTileService.TransformIntoDynamicTile(tile);
                }

                var result = false;

                if (tile is IDynamicTile dynamicTile)
                    result = dynamicTile.AddItem(item).Succeeded;

                if (result) {
                    if (!hasTable) Lua.PushNil(luaState);
                    continue;
                }
            } else {
                GetScriptEnv().AddTempItem(item);
                //todo: check if need this
                //item->setParent(VirtualCylinder::virtualCylinder);
            }

            if (hasTable) {
                Lua.PushNumber(luaState, i);
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
                Lua.SetTable(luaState, -3);
            } else {
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
            }
        }

        return 1;
    }
    
    // private static int LuaGameCreateMonster(LuaState luaState)
    // {
    //     // Game.createMonster(name, position, extended = false, force = false, master = nil)
    //     var name = GetString(luaState, 1);
    //     var position = GetPosition(luaState, 2);
    //     var extended = GetBoolean(luaState, 3, false);
    //     var force = GetBoolean(luaState, 4, false);
    //     var master = (Lua.GetTop(luaState) >= 5) ? GetUserdata<ICreature>(luaState, 5) : null;
    //     var monster = master is null ? _creatureFactory.CreateMonster(name) : _creatureFactory.CreateSummon(name, master);
    //     if (!monster)
    //     {
    //         // logger.Error("Monster {Name} does not exists.", name);
    //         Lua.PushNil(luaState);
    //         return 1;
    //     }
    //
    //     if (!monster) {
    //         Lua.PushNil(luaState);
    //         return 1;
    //     }
    //     
    //     var tileToBorn = _map[position];
    //     
    //     if (tileToBorn is IDynamicTile { HasCreature: false }) {
    //         if (tileToBorn.HasFlag(TileFlags.ProtectionZone)) {
    //             Lua.PushNil(luaState);
    //             return 1;
    //         }
    //     
    //         monster.Born(position);
    //     
    //         PushUserdata(luaState, monster);
    //         SetMetatable(luaState, -1, "Monster");
    //     
    //         return 1;
    //     }
    //     
    //     foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
    //         if (_map[neighbour] is IDynamicTile { HasCreature: false }) {
    //             monster.Born(neighbour);
    //     
    //             PushUserdata(luaState, monster);
    //             SetMetatable(luaState, -1, "Monster");
    //     
    //             return 1;
    //         }
    //     
    //     Lua.PushNil(luaState);
    //     return 1;
    // }
    
    private static int LuaGameCreateMonster(LuaState luaState)
    {
        // Game.createMonster(name, position, extended = false, force = false, master = nil)
        var name = GetString(luaState, 1);
        var position = GetPosition(luaState, 2);
        var extended = GetBoolean(luaState, 3, false);
        var force = GetBoolean(luaState, 4, false);
        var master = (Lua.GetTop(luaState) >= 5) ? GetUserdata<ICreature>(luaState, 5) : null;
        var monster = _createMonsterOrSummonUseCase.Execute(name, position, extended, force, master);
        
        if (monster is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }
        
        PushUserdata(luaState, monster);
        SetMetatable(luaState, -1, "Monster");
        return 1;
    }


    // private static int LuaGameCreateMonster(LuaState luaState) {
    //     // Game.createMonster(monsterName, position, extended = false, force = false, master = nil)
    //     //todo: implements force parameter
    //
    //     var monsterName = GetString(luaState, 1);
    //
    //     var position = GetPosition(luaState, 2);
    //     var extended = GetBoolean(luaState, 3, false);
    //     var force = GetBoolean(luaState, 4, false);
    //
    //     ICreature master = null;
    //
    //     var isSummon = false;
    //     if (Lua.GetTop(luaState) >= 5) {
    //         master = GetUserdata<ICreature>(luaState, 5);
    //         if (master.IsNotNull()) isSummon = true;
    //     }
    //
    //     IMonster monster = null;
    //     if (isSummon && master != null)
    //         monster = _creatureFactory.CreateSummon(monsterName, master);
    //     else
    //         monster = _creatureFactory.CreateMonster(monsterName);
    //
    //     if (!monster) {
    //         Lua.PushNil(luaState);
    //         return 1;
    //     }
    //
    //     var tileToBorn = _map[position];
    //
    //     if (tileToBorn is IDynamicTile { HasCreature: false }) {
    //         if (tileToBorn.HasFlag(TileFlags.ProtectionZone)) {
    //             Lua.PushNil(luaState);
    //             return 1;
    //         }
    //
    //         monster.Born(position);
    //
    //         PushUserdata(luaState, monster);
    //         SetMetatable(luaState, -1, "Monster");
    //
    //         return 1;
    //     }
    //
    //     foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
    //         if (_map[neighbour] is IDynamicTile { HasCreature: false }) {
    //             monster.Born(neighbour);
    //
    //             PushUserdata(luaState, monster);
    //             SetMetatable(luaState, -1, "Monster");
    //
    //             return 1;
    //         }
    //
    //     Lua.PushNil(luaState);
    //     return 1;
    // }

    private static int LuaGameCreateNpc(LuaState luaState) {
        // Game.createNpc(npcName, position, extended = false, force = false)
        //todo: implements force parameter

        var ncpName = GetString(luaState, 1);

        var position = GetPosition(luaState, 2);
        var extended = GetBoolean(luaState, 3, false);
        var force = GetBoolean(luaState, 4, false);

        var npc = _creatureFactory.CreateNpc(ncpName);

        if (!npc) {
            Lua.PushNil(luaState);
            return 1;
        }

        var tileToBorn = _map[position];

        if (tileToBorn is IDynamicTile { HasCreature: false }) {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone)) {
                Lua.PushNil(luaState);
                return 1;
            }

            npc.SetNewLocation(tileToBorn.Location);
            _map.PlaceCreature(npc);

            PushUserdata(luaState, npc);
            SetMetatable(luaState, -1, "Npc");

            return 1;
        }

        foreach (var neighbour in extended ? position.ExtendedNeighbours : position.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasCreature: false }) {
                npc.SetNewLocation(neighbour);
                _map.PlaceCreature(npc);

                PushUserdata(luaState, npc);
                SetMetatable(luaState, -1, "Npc");

                return 1;
            }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaGameReload(LuaState luaState) {
        // Game.reload(reloadType)
        var reloadType = GetNumber<ReloadType>(luaState, 1);
        if (reloadType == ReloadType.RELOAD_TYPE_NONE) {
            ReportError(nameof(LuaGameReload), "Reload type is none");
            PushBoolean(luaState, false);
            return 0;
        }

        if (reloadType >= ReloadType.RELOAD_TYPE_LAST) {
            ReportError(nameof(LuaGameReload), "Reload type not exist");
            PushBoolean(luaState, false);
            return 0;
        }

        try {
            var dir = AppContext.BaseDirectory + _serverConfiguration.DataLuaJit;
            switch (reloadType) {
                case ReloadType.RELOAD_TYPE_CORE: {
                    ReloadCore(dir);
                    break;
                }

                case ReloadType.RELOAD_TYPE_SCRIPTS: {
                    ReloadScripts(dir);
                    break;
                }

                default:
                    ReportError(nameof(LuaGameReload), "Reload type not implemented");
                    break;
            }

            Lua.GC(LuaEnvironment.GetInstance().GetLuaState(), LuaGCParam.Collect, 0);
        }
        catch (Exception e) {
            PushBoolean(luaState, false);
            return 0;
        }

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGameGetPlayers(LuaState luaState) {
        // Game.getPlayers()
        var allPlayers = _gameCreatureManager.GetAllLoggedPlayers();

        Lua.CreateTable(luaState, allPlayers.Count(), 0);

        int index = 0;
        foreach (var player in allPlayers) {
            PushUserdata(luaState, player);
            SetMetatable(luaState, -1, "Player");
            Lua.RawSetI(luaState, -2, ++index);
        }

        return 1;
    }

    private static void ReloadCore(string dir) {
        var coreLoaded = _luaEnvironment.LoadFile($"{dir}/core.lua", "core.lua");
        if (!coreLoaded) return;

        _scripts.LoadScripts($"{dir}/scripts/libs", true, false);
    }


    private static void ReloadScripts(string dir) {
        _scripts.ClearAllScripts();
        _scripts.LoadScripts($"{dir}/scripts", false, true);
        _scripts.LoadScripts($"{dir}/scripts/libs", true, true);
    }
}