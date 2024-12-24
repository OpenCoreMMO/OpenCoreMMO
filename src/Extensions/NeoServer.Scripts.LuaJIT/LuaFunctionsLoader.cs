using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Structs;

namespace NeoServer.Scripts.LuaJIT;

public class LuaFunctionsLoader
{
    public const int LUA_REGISTRYINDEX = (-10000);
    public const int LUA_ENVIRONINDEX = (-10001);
    public const int LUA_GLOBALSINDEX = (-10002);

    public static string GetErrorDesc(ErrorCodeType code)
    {
        switch (code)
        {
            case ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND:
                return "Player not found";
            case ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND:
                return "Creature not found";
            case ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND:
                return "Npc not found";
            case ErrorCodeType.LUA_ERROR_NPC_TYPE_NOT_FOUND:
                return "Npc type not found";
            case ErrorCodeType.LUA_ERROR_MONSTER_NOT_FOUND:
                return "Monster not found";
            case ErrorCodeType.LUA_ERROR_MONSTER_TYPE_NOT_FOUND:
                return "Monster type not found";
            case ErrorCodeType.LUA_ERROR_ITEM_NOT_FOUND:
                return "Item not found";
            case ErrorCodeType.LUA_ERROR_THING_NOT_FOUND:
                return "Thing not found";
            case ErrorCodeType.LUA_ERROR_TILE_NOT_FOUND:
                return "Tile not found";
            case ErrorCodeType.LUA_ERROR_HOUSE_NOT_FOUND:
                return "House not found";
            case ErrorCodeType.LUA_ERROR_COMBAT_NOT_FOUND:
                return "Combat not found";
            case ErrorCodeType.LUA_ERROR_CONDITION_NOT_FOUND:
                return "Condition not found";
            case ErrorCodeType.LUA_ERROR_AREA_NOT_FOUND:
                return "Area not found";
            case ErrorCodeType.LUA_ERROR_CONTAINER_NOT_FOUND:
                return "Container not found";
            case ErrorCodeType.LUA_ERROR_VARIANT_NOT_FOUND:
                return "Variant not found";
            case ErrorCodeType.LUA_ERROR_VARIANT_UNKNOWN:
                return "Unknown variant type";
            case ErrorCodeType.LUA_ERROR_SPELL_NOT_FOUND:
                return "Spell not found";
            case ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND:
                return "Action not found";
            case ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND:
                return "TalkAction not found";
            case ErrorCodeType.LUA_ERROR_ZONE_NOT_FOUND:
                return "Zone not found";
            default:
                return "Bad error code";
        }
    }

    public static int ProtectedCall(LuaState L, int nargs, int nresults)
    {
        var ret = 0;
        if (ValidateDispatcherContext(nameof(ProtectedCall)))
        {
            return ret;
        }

        int errorIndex = Lua.GetTop(L) - nargs;
        //int errorIndex = -1 - nargs - 1;
        Lua.PushCFunction(L, LuaErrorHandler);
        Lua.Insert(L, errorIndex);

        //int before = Lua.GetTop(L);

        ret = Lua.PCall(L, nargs, nresults, errorIndex);
        Lua.Remove(L, errorIndex);
        return ret;
    }

    public void ReportError(string errorDesc)
    {
        ReportError("__FUNCTION__", errorDesc);
    }

    public static void ReportError(string function, string errorDesc, bool stackTrace = false)
    {
        int scriptId, callbackId;
        bool timerEvent;
        LuaScriptInterface scriptInterface;

        GetScriptEnv().GetEventInfo(out scriptId, out scriptInterface, out callbackId, out timerEvent);

        //Logger.Error("Lua script error: \nscriptInterface: [{0}]\nscriptId: [{1}]\ntimerEvent: [{2}]\n callbackId:[{3}]\nfunction: [{4}]\nerror [{5}]",
        //             scriptInterface != null ? scriptInterface.GetInterfaceName() : "",
        //             scriptId != 0 ? scriptInterface.GetFileById(scriptId) : "",
        //             timerEvent ? "in a timer event called from:" : "",
        //             callbackId != 0 ? scriptInterface.GetFileById(callbackId) : "",
        //             function ?? "",
        //             (stackTrace && scriptInterface != null) ? scriptInterface.GetStackTrace(errorDesc) : errorDesc);

        Console.WriteLine(string.Format("Lua script error: \nscriptInterface: [{0}]\nscriptId: [{1}]\ntimerEvent: [{2}]\n callbackId:[{3}]\nfunction: [{4}]\nerror [{5}]",
                     scriptInterface != null ? scriptInterface.GetInterfaceName() : "",
                     scriptId != 0 ? scriptInterface.GetFileById(scriptId) : "",
                     timerEvent ? "in a timer event called from:" : "",
                     callbackId != 0 ? scriptInterface.GetFileById(callbackId) : "",
                     function ?? "",
                     (stackTrace && scriptInterface != null) ? scriptInterface.GetStackTrace(errorDesc) : errorDesc));
    }

    public static int LuaErrorHandler(LuaState L)
    {
        string errorMessage = PopString(L);
        LuaScriptInterface scriptInterface = GetScriptEnv().GetScriptInterface();
        Debug.Assert(scriptInterface != null); // This fires if the ScriptEnvironment hasn't been set up
        PushString(L, scriptInterface.GetStackTrace(errorMessage));
        return 1;
    }

    public static void PushVariant(LuaState L, LuaVariant var)
    {
        if (ValidateDispatcherContext(nameof(PushVariant)))
        {
            return;
        }

        Lua.CreateTable(L, 0, 4);
        SetField(L, "type", (double)var.Type);

        switch (var.Type)
        {
            case LuaVariantType.VARIANT_NUMBER:
                SetField(L, "number", var.Number);
                break;
            case LuaVariantType.VARIANT_STRING:
                SetField(L, "string", var.Text);
                break;
            case LuaVariantType.VARIANT_TARGETPOSITION:
            case LuaVariantType.VARIANT_POSITION:
                {
                    //TODO: Muniz
                    //PushPosition(L, var.Po);
                    //Lua.SetField(L, -2, "pos");
                    break;
                }
            default:
                break;
        }

        SetField(L, "instantName", var.InstantName);
        SetField(L, "runeName", var.RuneName);
        SetMetatable(L, -1, "Variant");
    }

    public static void PushThing(LuaState L, IThing thing)
    {
        if (ValidateDispatcherContext(nameof(PushThing)))
        {
            return;
        }

        if (thing == null)
        {
            Lua.CreateTable(L, 0, 4);
            SetField(L, "uid", 0);
            SetField(L, "itemid", 0);
            SetField(L, "actionid", 0);
            SetField(L, "type", 0);
            return;
        }

        if (thing is IItem item)
        {
            PushUserdata(L, item);
            SetItemMetatable(L, -1, item);
        }
        else if (thing is ICreature creature)
        {
            PushUserdata(L, creature);
            SetCreatureMetatable(L, -1, creature);
        }
        else
        {
            Lua.PushNil(L);
        }
    }

    //public static void PushCylinder(LuaState L, Cylinder cylinder)
    //{
    //    if (ValidateDispatcherContext(nameof(PushCylinder)))
    //    {
    //        return;
    //    }

    //    if (cylinder.GetCreature() is Creature creature)
    //    {
    //        PushUserdata(L, creature);
    //        SetCreatureMetatable(L, -1, creature);
    //    }
    //    else if (cylinder.GetItem() is Item parentItem)
    //    {
    //        PushUserdata(L, parentItem);
    //        SetItemMetatable(L, -1, parentItem);
    //    }
    //    else if (cylinder.GetTile() is Tile tile)
    //    {
    //        PushUserdata(L, tile);
    //        SetMetatable(L, -1, "Tile");
    //    }
    //    else if (cylinder == VirtualCylinder.VirtualCylinder)
    //    {
    //        PushBoolean(L, true);
    //    }
    //    else
    //    {
    //        Lua.PushNil(L);
    //    }
    //}

    public static void PushString(LuaState L, string value)
    {
        if (ValidateDispatcherContext(nameof(PushString)))
        {
            return;
        }

        Lua.PushLString(L, value, (ulong)value.Length);
    }

    public static void PushCallback(LuaState L, int callback)
    {
        if (ValidateDispatcherContext(nameof(PushCallback)))
        {
            return;
        }

        Lua.RawGetI(L, LUA_REGISTRYINDEX, callback);
    }

    public static string PopString(LuaState L)
    {
        if (Lua.GetTop(L) == 0)
        {
            return string.Empty;
        }

        string str = GetString(L, -1);
        Lua.Pop(L, 1);
        return str;
    }

    public static int PopCallback(LuaState L)
    {
        return Lua.Ref(L, LUA_REGISTRYINDEX);
    }

    // Metatables
    public static void SetMetatable(LuaState L, int index, string name)
    {
        if (ValidateDispatcherContext(nameof(SetMetatable)))
        {
            return;
        }

        Lua.GetMetaTable(L, name);
        Lua.SetMetaTable(L, index - 1);
    }

    public static void SetWeakMetatable(LuaState L, int index, string name)
    {
        HashSet<string> weakObjectTypes = new HashSet<string>();

        if (ValidateDispatcherContext(nameof(SetWeakMetatable)))
        {
            return;
        }

        string weakName = name + "_weak";

        if (weakObjectTypes.Add(name))
        {
            Lua.GetMetaTable(L, name);
            int childMetatable = Lua.GetTop(L);

            Lua.NewMetaTable(L, weakName);
            int metatable = Lua.GetTop(L);

            List<string> methodKeys = new List<string> { "__index", "__metatable", "__eq" };
            foreach (string metaKey in methodKeys)
            {
                Lua.GetField(L, childMetatable, metaKey);
                Lua.SetField(L, metatable, metaKey);
            }

            List<int> methodIndexes = new List<int> { 'h', 'p', 't' };
            foreach (int metaIndex in methodIndexes)
            {
                Lua.RawGetI(L, childMetatable, metaIndex);
                Lua.RawSetI(L, metatable, metaIndex);
            }

            Lua.PushNil(L);
            Lua.SetField(L, metatable, "__gc");

            Lua.Remove(L, childMetatable);
        }
        else
        {
            Lua.GetMetaTable(L, weakName);
        }

        Lua.SetMetaTable(L, index - 1);
    }

    public static void SetItemMetatable(LuaState L, int index, IItem item)
    {
        if (ValidateDispatcherContext(nameof(SetItemMetatable)))
        {
            return;
        }

        //if (item != null && item.GetContainer() != null)
        //{
        //    Lua.GetMetaTable(L, "Container");
        //}
        //else if (item != null && item.GetTeleport() != null)
        //{
        //    Lua.GetMetaTable(L, "Teleport");
        //}
        //else
        //{
        Lua.GetMetaTable(L, "Item");
        //}

        Lua.SetMetaTable(L, index - 1);
    }

    public static void SetCreatureMetatable(LuaState L, int index, ICreature creature)
    {
        if (ValidateDispatcherContext(nameof(SetCreatureMetatable)))
        {
            return;
        }

        if (creature != null && creature is IPlayer)
        {
            Lua.GetMetaTable(L, "Player");
        }
        else if (creature != null && creature is IMonster)
        {
            Lua.GetMetaTable(L, "Monster");
        }
        else
        {
            Lua.GetMetaTable(L, "Npc");
        }

        Lua.SetMetaTable(L, index - 1);
    }

    //public static CombatDamage GetCombatDamage(LuaState L)
    //{
    //    CombatDamage damage = new CombatDamage
    //    {
    //        Primary = { Value = GetNumber<int>(L, -4), Type = GetNumber<CombatType_t>(L, -3) },
    //        Secondary = { Value = GetNumber<int>(L, -2), Type = GetNumber<CombatType_t>(L, -1) }
    //    };

    //    Lua.Pop(L, 4);
    //    return damage;
    //}

    public static string GetFormatedLoggerMessage(LuaState L)
    {
        string format = GetString(L, 1);
        int n = Lua.GetTop(L);
        var args = new List<object>();

        for (int i = 2; i <= n; i++)
        {
            if (IsString(L, i))
            {
                args.Add(Lua.ToString(L, i));
            }
            else if (IsNumber(L, i))
            {
                args.Add(Lua.ToNumber(L, i));
            }
            else if (IsBoolean(L, i))
            {
                args.Add(Lua.ToBoolean(L, i) ? "true" : "false");
            }
            else if (IsUserdata(L, i))
            {
                LuaDataType userType = GetUserdataType(L, i);
                args.Add(GetUserdataTypeName(userType));
            }
            else if (IsTable(L, i))
            {
                args.Add("table");
            }
            else if (IsNil(L, i))
            {
                args.Add("nil");
            }
            else if (IsFunction(L, i))
            {
                args.Add("function");
            }
            else
            {
                //g_logger().warn("[{}] invalid param type", nameof(GetFormatedLoggerMessage));
                Console.WriteLine("[{0}] invalid param type", nameof(GetFormatedLoggerMessage));
            }
        }

        try
        {
            List<string> indexedArguments = args.Select((arg, index) => $"{{{index}}}").ToList();
            string formattedMessage = string.Format(format, indexedArguments.ToArray());
            return formattedMessage;
            //return fmt.vformat(format, args);
        }
        catch (Exception e)
        {
            //g_logger().error("[{}] format error: {}", nameof(GetFormatedLoggerMessage), e.what());
        }

        return string.Empty;
    }

    public static string GetString(LuaState L, int arg)
    {
        ulong len = 0;
        var c_str = Lua.ToLString(L, arg, ref len);
        if (c_str == null || len == 0)
        {
            return "";
        }
        return c_str;
    }

    public static Location GetPosition(LuaState L, int arg, out int stackpos)
    {
        Location position = new Location
        {
            X = GetField<ushort>(L, arg, "x"),
            Y = GetField<ushort>(L, arg, "y"),
            Z = GetField<byte>(L, arg, "z")
        };

        Lua.GetField(L, arg, "stackpos");
        if (Lua.IsNil(L, -1))
        {
            stackpos = 0;
        }
        else
        {
            stackpos = GetNumber<int>(L, -1);
        }

        Lua.Pop(L, 4);
        return position;
    }

    public static Location GetPosition(LuaState L, int arg)
    {
        Location position = new Location
        {
            X = GetField<ushort>(L, arg, "x"),
            Y = GetField<ushort>(L, arg, "y"),
            Z = GetField<byte>(L, arg, "z")
        };

        Lua.Pop(L, 3);
        return position;
    }

    //public static Outfit_t GetOutfit(LuaState L, int arg)
    //{
    //    Outfit_t outfit = new Outfit_t
    //    {
    //        lookMountFeet = GetField<uint8_t>(L, arg, "lookMountFeet"),
    //        lookMountLegs = GetField<uint8_t>(L, arg, "lookMountLegs"),
    //        lookMountBody = GetField<uint8_t>(L, arg, "lookMountBody"),
    //        lookMountHead = GetField<uint8_t>(L, arg, "lookMountHead"),
    //        lookFamiliarsType = GetField<uint16_t>(L, arg, "lookFamiliarsType"),
    //        lookMount = GetField<uint16_t>(L, arg, "lookMount"),
    //        lookAddons = GetField<uint8_t>(L, arg, "lookAddons"),

    //        lookFeet = GetField<uint8_t>(L, arg, "lookFeet"),
    //        lookLegs = GetField<uint8_t>(L, arg, "lookLegs"),
    //        lookBody = GetField<uint8_t>(L, arg, "lookBody"),
    //        lookHead = GetField<uint8_t>(L, arg, "lookHead"),

    //        lookTypeEx = GetField<uint16_t>(L, arg, "lookTypeEx"),
    //        lookType = GetField<uint16_t>(L, arg, "lookType")
    //    };

    //    Lua.Pop(L, 13);
    //    return outfit;
    //}

    public static LuaVariant GetVariant(LuaState L, int arg)
    {
        LuaVariant var = new LuaVariant
        {
            InstantName = GetFieldString(L, arg, "instantName"),
            RuneName = GetFieldString(L, arg, "runeName"),
            Type = GetField<LuaVariantType>(L, arg, "type")
        };

        switch (var.Type)
        {
            case LuaVariantType.VARIANT_NUMBER:
                var.Number = GetField<uint>(L, arg, "number");
                Lua.Pop(L, 4);
                break;

            case LuaVariantType.VARIANT_STRING:
                var.Text = GetFieldString(L, arg, "string");
                Lua.Pop(L, 4);
                break;

            case LuaVariantType.VARIANT_POSITION:
            case LuaVariantType.VARIANT_TARGETPOSITION:
                Lua.GetField(L, arg, "pos");
                //TODO: MUNIZ
                //var.Pos = GetPosition(L, Lua.GetTop(L));
                Lua.Pop(L, 4);
                break;

            default:
                var.Type = LuaVariantType.VARIANT_NONE;
                Lua.Pop(L, 3);
                break;
        }

        return var;
    }

    //public static Thing GetThing(LuaState L, int arg)
    //{
    //    Thing thing;
    //    if (Lua.GetMetaTable(L, arg) != 0)
    //    {
    //        Lua.RawGetI(L, -1, 't');
    //        switch (GetNumber<LuaDataType>(L, -1))
    //        {
    //            case LuaDataType.Item:
    //                thing = GetUserdataShared<Item>(L, arg);
    //                break;
    //            case LuaDataType.Container:
    //                thing = GetUserdataShared<Container>(L, arg);
    //                break;
    //            case LuaDataType.Teleport:
    //                thing = GetUserdataShared<Teleport>(L, arg);
    //                break;
    //            case LuaDataType.Player:
    //                thing = GetUserdataShared<Player>(L, arg);
    //                break;
    //            case LuaDataType.Monster:
    //                thing = GetUserdataShared<Monster>(L, arg);
    //                break;
    //            case LuaDataType.Npc:
    //                thing = GetUserdataShared<Npc>(L, arg);
    //                break;
    //            default:
    //                thing = null;
    //                break;
    //        }
    //        Lua.Pop(L, 2);
    //    }
    //    else
    //    {
    //        thing = GetScriptEnv().GetThingByUID(GetNumber<uint32_t>(L, arg));
    //    }
    //    return thing;
    //}

    //public static shared_ptr<Creature> GetCreature(LuaState L, int arg)
    //{
    //    if (IsUserdata(L, arg))
    //    {
    //        return GetUserdataShared<Creature>(L, arg);
    //    }
    //    return g_game().GetCreatureByID(GetNumber<uint32_t>(L, arg));
    //}

    //public static shared_ptr<Player> GetPlayer(LuaState L, int arg, bool allowOffline = false)
    //{
    //    if (IsUserdata(L, arg))
    //    {
    //        return GetUserdataShared<Player>(L, arg);
    //    }
    //    else if (IsNumber(L, arg))
    //    {
    //        return g_game().GetPlayerByID(GetNumber<uint64_t>(L, arg), allowOffline);
    //    }
    //    else if (IsString(L, arg))
    //    {
    //        return g_game().GetPlayerByName(GetString(L, arg), allowOffline);
    //    }
    //    g_logger().Warn("LuaFunctionsLoader::GetPlayer: Invalid argument.");
    //    return null;
    //}

    //public static shared_ptr<Guild> GetGuild(LuaState L, int arg, bool allowOffline = false)
    //{
    //    if (IsUserdata(L, arg))
    //    {
    //        return GetUserdataShared<Guild>(L, arg);
    //    }
    //    else if (IsNumber(L, arg))
    //    {
    //        return g_game().GetGuild(GetNumber<uint64_t>(L, arg), allowOffline);
    //    }
    //    else if (IsString(L, arg))
    //    {
    //        return g_game().GetGuildByName(GetString(L, arg), allowOffline);
    //    }
    //    g_logger().Warn("LuaFunctionsLoader::GetGuild: Invalid argument.");
    //    return null;
    //}

    public static string GetFieldString(LuaState L, int arg, string key)
    {
        Lua.GetField(L, arg, key);
        return GetString(L, -1);
    }

    public static LuaDataType GetUserdataType(LuaState L, int arg)
    {
        if (Lua.GetMetaTable(L, arg) == 0)
        {
            return LuaDataType.Unknown;
        }
        Lua.RawGetI(L, -1, 't');

        LuaDataType type = GetNumber<LuaDataType>(L, -1);
        Lua.Pop(L, 2);

        return type;
    }

    public static string GetUserdataTypeName(LuaDataType userType)
    {
        //return magic_enum::enum_name(userType).data();
        return userType.ToString();
    }

    // Push
    public static void PushBoolean(LuaState L, bool value)
    {
        if (ValidateDispatcherContext(nameof(PushBoolean)))
        {
            return;
        }

        Lua.PushBoolean(L, value);
    }

    //public static void PushCombatDamage(LuaState L, CombatDamage damage)
    //{
    //    if (ValidateDispatcherContext(nameof(PushCombatDamage)))
    //    {
    //        return;
    //    }

    //    Lua.PushNumber(L, damage.primary.value);
    //    Lua.PushNumber(L, damage.primary.type);
    //    Lua.PushNumber(L, damage.secondary.value);
    //    Lua.PushNumber(L, damage.secondary.type);
    //    Lua.PushNumber(L, damage.origin);
    //}

    //public static void PushInstantSpell(LuaState L, InstantSpell spell)
    //{
    //    if (ValidateDispatcherContext(__FUNCTION__))
    //    {
    //        return;
    //    }

    //    Lua.CreateTable(L, 0, 6);

    //    SetField(L, "name", spell.GetName());
    //    SetField(L, "words", spell.GetWords());
    //    SetField(L, "level", spell.GetLevel());
    //    SetField(L, "mlevel", spell.GetMagicLevel());
    //    SetField(L, "mana", spell.GetMana());
    //    SetField(L, "manapercent", spell.GetManaPercent());

    //    SetMetatable(L, -1, "Spell");
    //}

    public static void PushPosition(LuaState L, Location position, int stackpos = 0)
    {
        if (ValidateDispatcherContext(nameof(PushPosition)))
        {
            return;
        }

        Lua.CreateTable(L, 0, 4);

        SetField(L, "x", position.X);
        SetField(L, "y", position.Y);
        SetField(L, "z", position.Z);
        SetField(L, "stackpos", stackpos);

        SetMetatable(L, -1, "Position");
    }

    //public static void PushOutfit(LuaState L, Outfit_t outfit)
    //{
    //    if (ValidateDispatcherContext(__FUNCTION__))
    //    {
    //        return;
    //    }

    //    Lua.CreateTable(L, 0, 13);
    //    SetField(L, "lookType", outfit.lookType);
    //    SetField(L, "lookTypeEx", outfit.lookTypeEx);
    //    SetField(L, "lookHead", outfit.lookHead);
    //    SetField(L, "lookBody", outfit.lookBody);
    //    SetField(L, "lookLegs", outfit.lookLegs);
    //    SetField(L, "lookFeet", outfit.lookFeet);
    //    SetField(L, "lookAddons", outfit.lookAddons);
    //    SetField(L, "lookMount", outfit.lookMount);
    //    SetField(L, "lookMountHead", outfit.lookMountHead);
    //    SetField(L, "lookMountBody", outfit.lookMountBody);
    //    SetField(L, "lookMountLegs", outfit.lookMountLegs);
    //    SetField(L, "lookMountFeet", outfit.lookMountFeet);
    //    SetField(L, "lookFamiliarsType", outfit.lookFamiliarsType);
    //}

    public static void RegisterClass(LuaState L, string className, string baseClass, LuaFunction newFunction = null)
    {
        // className = {}
        Lua.NewTable(L);
        Lua.PushValue(L, -1);
        Lua.SetGlobal(L, className);
        int methods = Lua.GetTop(L);

        // methodsTable = {}
        Lua.NewTable(L);
        int methodsTable = Lua.GetTop(L);

        if (newFunction != null)
        {
            // className.__call = newFunction
            Lua.PushCFunction(L, newFunction);
            Lua.SetField(L, methodsTable, "__call");
        }

        uint parents = 0;
        if (!string.IsNullOrEmpty(baseClass))
        {
            Lua.GetGlobal(L, baseClass);
            Lua.RawGetI(L, -1, 'p');
            parents = GetNumber<uint>(L, -1) + 1;
            Lua.Pop(L, 1);
            Lua.SetField(L, methodsTable, "__index");
        }

        // setmetatable(className, methodsTable)
        Lua.SetMetaTable(L, methods);

        // className.metatable = {}
        Lua.NewMetaTable(L, className);
        int metatable = Lua.GetTop(L);

        // className.metatable.__metatable = className
        Lua.PushValue(L, methods);
        Lua.SetField(L, metatable, "__metatable");

        // className.metatable.__index = className
        Lua.PushValue(L, methods);
        Lua.SetField(L, metatable, "__index");

        // className.metatable['h'] = hash
        Lua.PushNumber(L, (double)className.GetHashCode());
        Lua.RawSetI(L, metatable, 'h');

        // className.metatable['p'] = parents
        Lua.PushNumber(L, parents);
        Lua.RawSetI(L, metatable, 'p');

        // className.metatable['t'] = type
        Enum.TryParse<LuaDataType>(className, true, out var userTypeEnum);
        if (userTypeEnum != null)
        {
            Lua.PushNumber(L, (double)userTypeEnum);
        }
        else
        {
            Lua.PushNumber(L, (double)LuaDataType.Unknown);
        }
        Lua.RawSetI(L, metatable, 't');

        // pop className, className.metatable
        Lua.Pop(L, 2);
    }

    public static void RegisterMethod(LuaState L, string globalName, string methodName, LuaFunction func)
    {
        // globalName.methodName = func
        Lua.GetGlobal(L, globalName);
        Lua.PushCFunction(L, func);
        Lua.SetField(L, -2, methodName);

        // pop globalName
        Lua.Pop(L, 1);
    }

    public static void RegisterTable(LuaState L, string tableName)
    {
        // _G[tableName] = {}
        Lua.NewTable(L);
        Lua.SetGlobal(L, tableName);
    }

    public static void RegisterMetaMethod(LuaState L, string className, string methodName, LuaFunction func)
    {
        // className.metatable.methodName = func
        Lua.GetMetaTable(L, className);
        Lua.PushCFunction(L, func);
        Lua.SetField(L, -2, methodName);

        // pop className.metatable
        Lua.Pop(L, 1);
    }

    public static void RegisterVariable(LuaState L, string tableName, string name, double value)
    {
        // tableName.name = value
        Lua.GetGlobal(L, tableName);
        SetField(L, name, value);

        // pop tableName
        Lua.Pop(L, 1);
    }

    public static void RegisterVariable(LuaState L, string tableName, string name, string value)
    {
        // tableName.name = value
        Lua.GetGlobal(L, tableName);
        SetField(L, name, value);

        // pop tableName
        Lua.Pop(L, 1);
    }
    public static void RegisterVariable(LuaState L, string tableName, string name, BooleanConfigType value)
    {
        RegisterVariable(L, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState L, string tableName, string name, StringConfigType value)
    {
        RegisterVariable(L, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState L, string tableName, string name, IntegerConfigType value)
    {
        RegisterVariable(L, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState L, string tableName, string name, FloatingConfigType value)
    {
        RegisterVariable(L, tableName, name, (double)value);
    }

    public static void RegisterGlobalBoolean(LuaState L, string name, bool value)
    {
        // _G[name] = value
        PushBoolean(L, value);
        Lua.SetGlobal(L, name);
    }

    public static void RegisterGlobalMethod(LuaState L, string functionName, LuaFunction func)
    {
        // _G[functionName] = func
        Lua.PushCFunction(L, func);
        Lua.SetGlobal(L, functionName);
    }

    public static void RegisterGlobalVariable(LuaState L, string name, double value)
    {
        // _G[name] = value
        Lua.PushNumber(L, value);
        Lua.SetGlobal(L, name);
    }

    public static void RegisterGlobalVariable(LuaState L, string name, ReloadType value)
    {
        // _G[name] = value
        RegisterGlobalVariable(L, name, (double)value);
    }

    public static void RegisterGlobalVariable(LuaState L, string name, MessageClassesType value)
    {
        // _G[name] = value
        RegisterGlobalVariable(L, name, (double)value);
    }

    public static void RegisterGlobalString(LuaState L, string variable, string name)
    {
        // Example: RegisterGlobalString(L, "VARIABLE_NAME", "variable string");
        PushString(L, name);
        Lua.SetGlobal(L, variable);
    }

    public static string EscapeString(string str)
    {
        string s = str;
        s.Replace("\\", "\\\\");
        s.Replace("\"", "\\\"");
        s.Replace("'", "\\'");
        s.Replace("[[", "\\[[");
        return s;
    }

    //public static int LuaUserdataCompare(LuaState L)
    //{
    //    PushBoolean(L, GetUserdata<void>(L, 1) == GetUserdata<void>(L, 2));
    //    return 1;
    //}


    public static int LuaUserdataCompare<T>(LuaState L) where T : class
    {
        PushBoolean(L, GetUserdata<T>(L, 1) == GetUserdata<T>(L, 2));
        return 1;
    }

    public static int LuaUserdataCompareStruct<T>(LuaState L) where T : struct
    {
        PushBoolean(L, EqualityComparer<T>.Default.Equals(GetUserdataStruct<T>(L, 1), GetUserdataStruct<T>(L, 2)));
        return 1;
    }

    public static void RegisterSharedClass(LuaState L, string className, string baseClass, LuaFunction newFunction)
    {
        RegisterClass(L, className, baseClass, newFunction);
        RegisterMetaMethod(L, className, "__gc", LuaGarbageCollection);
    }

    public static int LuaGarbageCollection(LuaState L)
    {
        try
        {
            IntPtr userdata = (IntPtr)Lua.ToUserData(L, 1);
            var stru = (UserDataStruct)Marshal.PtrToStructure(Marshal.ReadIntPtr(userdata), typeof(UserDataStruct));

            if (_objects.ContainsKey(stru.Index))
            {
                var obj = _objects[stru.Index];
                _objects.Remove(stru.Index);
                _objectsBackMap.Remove(obj);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch (Exception e)
        {
            //Logger.GetInstance().Error($"Exception in GarbageCollection: {e.InnerException}");
        }

        return 0;
    }

    public static bool ValidateDispatcherContext(string fncName)
    {
        //if (g_dispatcher().context().isOn() && g_dispatcher().context().isAsync())
        //{
        //    g_logger().warn("[{}] The call to lua was ignored because the '{}' task is trying to communicate while in async mode.", fncName, g_dispatcher().context().getName());
        //    return LUA_ERRRUN > 0;
        //}

        //return 0;

        return false;
    }

    private static int scriptEnvIndex = 0;
    private static ScriptEnvironment[] scriptEnv = new ScriptEnvironment[16];

    //public static void pushUserdata<T>(LuaState L, T value) where T : class
    //{
    //    IntPtr userdata = (nint)Lua.NewUserData(L, (uint)IntPtr.Size);
    //    System.Runtime.InteropServices.Marshal.WriteIntPtr(userdata, System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(new T[] { value }, 0));
    //}

    public static T GetNumber<T>(LuaState L, int arg) where T : struct
    {
        if (typeof(T).IsEnum)
        {
            return (T)Enum.ToObject(typeof(T), (long)Lua.ToNumber(L, arg));
        }
        else if (typeof(T).IsPrimitive)
        {
            return (T)Convert.ChangeType(Lua.ToNumber(L, arg), typeof(T));
        }
        else
        {
            throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }
    }

    public static T GetNumber<T>(LuaState L, int arg, T defaultValue) where T : struct
    {
        int parameters = Lua.GetTop(L);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return GetNumber<T>(L, arg);
    }

    public static T GetUserdataShared<T>(LuaState L, int arg) where T : struct
    {
        IntPtr userdata = (IntPtr)Lua.ToUserData(L, arg);

        if (userdata == IntPtr.Zero)
        {
            return default(T);
        }

        IntPtr ptr = Marshal.ReadIntPtr(userdata);
        return (T)Marshal.PtrToStructure(ptr, typeof(T));
    }

    public enum LuaType
    {
        /// <summary>
        /// 
        /// </summary>
        None = -1,
        /// <summary>
        /// LUA_TNIL
        /// </summary>
        Nil = 0,
        /// <summary>
        /// LUA_TBOOLEAN
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// LUA_TLIGHTUSERDATA
        /// </summary>
        LightUserData = 2,
        /// <summary>
        /// LUA_TNUMBER
        /// </summary>
        Number = 3,
        /// <summary>
        /// LUA_TSTRING
        /// </summary>
        String = 4,
        /// <summary>
        /// LUA_TTABLE
        /// </summary>
        Table = 5,
        /// <summary>
        /// LUA_TFUNCTION
        /// </summary>
        Function = 6,
        /// <summary>
        /// LUA_TUSERDATA
        /// </summary>
        UserData = 7,
        /// <summary>
        /// LUA_TTHREAD
        /// </summary>
        /// //
        Thread = 8,
    }

    public static int ToNetObject(LuaState state, int index, IntPtr tag)
    {
        //if (state.Type(index) != LuaType.UserData)
        //    return -1;

        IntPtr userData;

        if (Lua.GetMetaTable(state, index) != 0)
        {
            userData = (IntPtr)Lua.ToUserData(state, index);
            if (userData != IntPtr.Zero)
                return Marshal.ReadInt32(userData);
        }

        //userData = state.CheckUData(index, "luaNet_class");
        //if (userData != IntPtr.Zero)
        //    return Marshal.ReadInt32(userData);

        //userData = state.CheckUData(index, "luaNet_searchbase");
        //if (userData != IntPtr.Zero)
        //    return Marshal.ReadInt32(userData);

        //userData = state.CheckUData(index, "luaNet_function");
        //if (userData != IntPtr.Zero)
        //    return Marshal.ReadInt32(userData);

        return -1;
    }

    public static T GetUserdata<T>(LuaState L, int arg) where T : class
    {
        var userdata = GetRawUserdata<T>(L, arg);
        if (userdata == IntPtr.Zero)
        {
            return null;
        }

        var stru = (UserDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(System.Runtime.InteropServices.Marshal.ReadIntPtr(userdata), typeof(UserDataStruct));

        return (T)_objects[stru.Index];
    }

    public static T GetUserdataStruct<T>(LuaState L, int arg) where T : struct
    {
        var userdata = GetRawUserdataStruct<T>(L, arg);
        if (userdata == IntPtr.Zero)
        {
            return default;
        }

        var stru = (UserDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(System.Runtime.InteropServices.Marshal.ReadIntPtr(userdata), typeof(UserDataStruct));

        return (T)_objects[stru.Index];
    }

    //public static void RemoveUserdata<T>() where T : class
    //{
    //    var key = typeof(T);
    //    if (_userData.ContainsKey(key))
    //        _userData.Remove(key);

    //    //IntPtr userdata = GetRawUserdata<T>(L, arg);
    //    //if (userdata == IntPtr.Zero)
    //    //{
    //    //    return null;
    //    //}
    //    //return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(System.Runtime.InteropServices.Marshal.ReadIntPtr(userdata), typeof(T));
    //}

    public static IntPtr GetRawUserdata<T>(LuaState L, int arg) where T : class
    {
        return (nint)Lua.ToUserData(L, arg);
    }

    public static IntPtr GetRawUserdataStruct<T>(LuaState L, int arg) where T : struct
    {
        return (nint)Lua.ToUserData(L, arg);
    }

    public static bool GetBoolean(LuaState L, int arg)
    {
        return Lua.ToBoolean(L, arg);
    }

    public static bool GetBoolean(LuaState L, int arg, bool defaultValue)
    {
        int parameters = Lua.GetTop(L);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return Lua.ToBoolean(L, arg);
    }

    public static string GetString(LuaState L, int arg, string defaultValue)
    {
        int parameters = Lua.GetTop(L);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return GetString(L, arg);
    }

    public static T GetField<T>(LuaState L, int arg, string key) where T : struct
    {
        Lua.GetField(L, arg, key);
        return GetNumber<T>(L, -1);
    }

    public static bool IsNumber(LuaState L, int arg)
    {
        return Lua.Type(L, arg) == LuaNET.LuaType.Number;
    }

    public static bool IsString(LuaState L, int arg)
    {
        return Lua.IsString(L, arg);
    }

    public static bool IsBoolean(LuaState L, int arg)
    {
        return Lua.IsBoolean(L, arg);
    }

    public static bool IsTable(LuaState L, int arg)
    {
        return Lua.IsTable(L, arg);
    }

    public static bool IsFunction(LuaState L, int arg)
    {
        return Lua.IsFunction(L, arg);
    }

    public static bool IsNil(LuaState L, int arg)
    {
        return Lua.IsNil(L, arg);
    }

    public static bool IsUserdata(LuaState L, int arg)
    {
        return Lua.IsUserData(L, arg);
    }

    public static void SetField(LuaState L, string index, double value)
    {
        Lua.PushNumber(L, value);
        Lua.SetField(L, -2, index);
    }

    public static void SetField(LuaState L, string index, string value)
    {
        PushString(L, value);
        Lua.SetField(L, -2, index);
    }

    public LuaFunctionsLoader()
    {
        for (int i = 0; i < scriptEnv.Length; i++)
        {
            scriptEnv[i] = new ScriptEnvironment();
        }
    }

    public ScriptEnvironment InternalGetScriptEnv()
    {
        if (scriptEnvIndex < 0 || scriptEnvIndex >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return scriptEnv[scriptEnvIndex];
    }

    public bool InternalReserveScriptEnv()
    {
        return ++scriptEnvIndex < 16;
    }

    public static ScriptEnvironment GetScriptEnv()
    {
        if (scriptEnvIndex < 0 || scriptEnvIndex >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return scriptEnv[scriptEnvIndex];
    }

    public static bool ReserveScriptEnv()
    {
        return ++scriptEnvIndex < 16;
    }

    public static void ResetScriptEnv()
    {
        if (scriptEnvIndex < 0)
        {
            throw new IndexOutOfRangeException();
        }
        scriptEnv[scriptEnvIndex--].ResetEnv();
    }

    /// <summary>
    /// Compatibility NewIndexedUserData with constant parameter
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static IntPtr NewUserData(LuaState L, int size)
    {
        return (IntPtr)Lua.NewUserData(L, (UIntPtr)size);
    }

    public static void NewUData(LuaState L, int val)
    {
        IntPtr pointer = NewUserData(L, Marshal.SizeOf(typeof(int)));
        Marshal.WriteInt32(pointer, val);
    }

    public static T ToObject<T>(LuaState L, int index, bool freeGCHandle = true)
    {
        //if (IsNil(index) || !IsLightUserData(index))
        //    return default(T);

        IntPtr data = (IntPtr)Lua.ToUserData(L, index);
        if (data == IntPtr.Zero)
            return default(T);

        var handle = GCHandle.FromIntPtr(data);
        if (!handle.IsAllocated)
            return default(T);

        var reference = (T)handle.Target;

        if (freeGCHandle)
            handle.Free();

        return reference;
    }

    public static void PushUserdata2<T>(LuaState L, T value, int a) where T : class
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            IntPtr userdata = (IntPtr)Lua.NewUserData(L, (ulong)IntPtr.Size);

            ptr = Marshal.UnsafeAddrOfPinnedArrayElement(new T[] { value }, 0);
            Marshal.WriteIntPtr(userdata, ptr);
        }
        catch (Exception ex)
        {

        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    Marshal.FreeHGlobal(ptr);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

    public static void PushUserdata<T>(LuaState L, T value, int a) where T : struct
    {
        IntPtr ptr = IntPtr.Zero;
        try
        {
            //IntPtr userdata2 = (IntPtr)Lua.NewUserData(L, (ulong)IntPtr.Size);

            //ptr = Marshal.UnsafeAddrOfPinnedArrayElement(new T[] { value }, 0);
            //Marshal.WriteIntPtr(userdata2, ptr);

            //TextWriter tw = Console.Out;
            //GCHandle gch = GCHandle.Alloc(tw);
            GCHandle gch2 = GCHandle.Alloc(value, GCHandleType.Pinned);
            //gch.Free();

            IntPtr userdata = (IntPtr)Lua.NewUserData(L, (ulong)IntPtr.Size);

            //var a = GCHandle.Alloc(value, GCHandleType.Pinned);

            //fixed (T* aptr = &value)
            //{
            //    Console.WriteLine(*aptr);
            //}

            //ptr = Marshal.UnsafeAddrOfPinnedArrayElement(new T[] { value }, 0);
            //ptr = GCHandle.Alloc(value).AddrOfPinnedObject();
            Marshal.WriteIntPtr(userdata, gch2.AddrOfPinnedObject());
            //Marshal.WriteIntPtr(userdata, ptr);
            gch2.Free();
        }
        catch (Exception ex)
        {

        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    //Marshal.FreeHGlobal(ptr);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

    // Compare cache entries by exact reference to avoid unwanted aliases
    private class ReferenceComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (x != null && y != null && x.GetType() == y.GetType() && x.GetType().IsValueType && y.GetType().IsValueType)
                return x.Equals(y); // Special case for boxed value types
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    // object to object #
    static readonly Dictionary<object, int> _objectsBackMap = new Dictionary<object, int>(new ReferenceComparer());
    // object # to object (FIXME - it should be possible to get object address as an object #)
    static readonly Dictionary<int, object> _objects = new Dictionary<int, object>();
    static readonly List<UserDataStruct> _structuresToReuse = new List<UserDataStruct>();

    static readonly ConcurrentQueue<int> finalizedReferences = new ConcurrentQueue<int>();

    //internal EventHandlerContainer PendingEvents = new EventHandlerContainer();

    /// <summary>
    /// We want to ensure that objects always have a unique ID
    /// </summary>
    static int _nextObj;

    private static int AddObject(object obj)
    {
        // New object: inserts it in the list
        int index = _nextObj++;
        _objects[index] = obj;

        if (!obj.GetType().IsValueType || obj.GetType().IsEnum)
            _objectsBackMap[obj] = index;

        return index;
    }

    public static void PushUserdata(LuaState L, object o)
    {
        int index = -1;

        // Pushes nil
        if (o == null)
        {
            Lua.PushNil(L);
            return;
        }

        // Object already in the list of Lua objects? Push the stored reference.
        bool found = (!o.GetType().IsValueType || o.GetType().IsEnum) && _objectsBackMap.TryGetValue(o, out index);

        index = AddObject(o);

        IntPtr userdata;

        UserDataStruct stru;

        if (_structuresToReuse.Any())
        {
            stru = _structuresToReuse.FirstOrDefault();
            _structuresToReuse.Remove(stru);

            stru.Index = index;
            userdata = stru.Ptr;
        }
        else
        {
            userdata = (IntPtr)Lua.NewUserData(L, (ulong)IntPtr.Size);

            // Registre o userdata no registry Lua
            //Lua.PushValue(L, -1);  // Duplicar o userdata
            //int referenceIndex = Lua.Ref(L, LUA_REGISTRYINDEX);

            stru = new UserDataStruct(index, userdata, 0);
        }

        GCHandle gch2 = GCHandle.Alloc(stru, GCHandleType.Pinned);

        Marshal.WriteIntPtr(userdata, gch2.AddrOfPinnedObject());
        gch2.Free();

        //lua_setupvalue

        //var obj1 = _objects[index];
        //var obj2 = _objects[userdata];

        //IntPtr pointer = (IntPtr)Lua.NewUserData(L, (ulong)Marshal.SizeOf(typeof(int)));
        //Marshal.WriteInt32(pointer, index);

        //_userData.Add(typeof(T), value);

        //IntPtr ptr = IntPtr.Zero;
        //try
        //{
        //    IntPtr userdata = (IntPtr)Lua.NewUserData(L, (ulong)IntPtr.Size);

        //    ptr = Marshal.UnsafeAddrOfPinnedArrayElement(new T[] { value }, 0);
        //    Marshal.WriteIntPtr(userdata, ptr);
        //}
        //catch(Exception ex)
        //{

        //}
        //finally
        //{
        //    if (ptr != IntPtr.Zero)
        //    {
        //        try
        //        {
        //            Marshal.FreeHGlobal(ptr);
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //}
    }

    //public static void PushUserdata(LuaState L, TalkAction value)
    //{
    //    var userdata = Lua.NewUserData(L, (ulong)IntPtr.Size);
    //    Marshal.StructureToPtr(value, (nint)userdata, false);

    //    //System.Runtime.InteropServices.Marshal.StructureToPtr(value, userData, false);
    //    //var userData = (IntPtr)Lua.NewUserData(L, sizeof(T));
    //}

    //public static void PushUserdata<T>(LuaState L, T value) where T : class
    //{
    //    var userdata = Lua.NewUserData(L, (ulong)IntPtr.Size);
    //    //Marshal.StructureToPtr(value, userdata, false);

    //    //System.Runtime.InteropServices.Marshal.StructureToPtr(value, userData, false);
    //    //var userData = (IntPtr)Lua.NewUserData(L, sizeof(T));
    //}

    //public static void PushUserdata(LuaState L, IntPtr value)
    //{
    //    var userData = (IntPtr)Lua.NewUserData(L, sizeof(value));
    //}

    //public static void PushString(LuaState L, string value)
    //{
    //    lua_pushstring(L, value);
    //}
}
