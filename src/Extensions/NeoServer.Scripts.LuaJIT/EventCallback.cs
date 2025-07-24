using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class EventCallback : Script
{
    private readonly ILogger _logger;
    private readonly IScripts _scripts;

    public EventCallback(LuaScriptInterface scriptInterface, ILogger logger, IScripts scripts) : base(scriptInterface)
    {
        _logger = logger;
        _scripts = scripts;
    }

    public EventCallbackType EventType { get; set; }
    public string ScriptTypeName { get; set; }

    public string CallbackName { get; set; }
    public bool SkipDuplicationCheck { get; set; }

    public bool LoadScriptId()
    {
        var luaInterface = _scripts.GetScriptInterface();
        SetScriptId(luaInterface.GetEvent());
        if (GetScriptId() == -1)
        {
            _logger.Error(
                "[CreatureEvent::LoadScriptId] Failed to load event. Script name: '{ScriptName}', Module: '{ModuleName}'",
                luaInterface.GetLoadingScriptName(), luaInterface.GetInterfaceName());
            return false;
        }

        return true;
    }

    public bool IsLoadedScriptId()
    {
        return GetScriptId() != 0;
    }

    #region Player Functions

    public void PlayerOnInventoryUpdate(IPlayer player, IItem item, Slot slot, bool equip)
    {
        // onInventoryUpdate(player, item, slot, equip)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error("[EventCallback::PlayerOnInventoryUpdate] Call stack overflow");
            return;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        LuaScriptInterface.PushUserdata(L, item);
        LuaScriptInterface.SetItemMetatable(L, -1, item);

        Lua.PushNumber(L, (byte)slot);
        LuaScriptInterface.PushBoolean(L, equip);

        scriptInterface.CallVoidFunction(4);
    }

    public bool PlayerOnRotateItem(IPlayer player, IItem item, Location position)
    {
        // onRotateItem(player, item, position)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error("[EventCallback::PlayerOnRotateItem] Call stack overflow");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        LuaScriptInterface.PushUserdata(L, item);
        LuaScriptInterface.SetItemMetatable(L, -1, item);

        LuaScriptInterface.PushPosition(L, position);

        return scriptInterface.CallFunction(3);
    }

    public void PlayerOnWalk(IPlayer player, byte direction)
    {
        // onWalk(player, direction)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(
                "[EventCallback::PlayerOnWalk - Player {PlayerName}] Call stack overflow. Too many lua script calls being nested.",
                player.Name);
            return;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");
        Lua.PushNumber(L, direction);

        scriptInterface.CallVoidFunction(2);
    }

    public void PlayerOnStorageUpdate(IPlayer player, uint key, int value, int oldValue, ulong currentTime)
    {
        // onStorageUpdate(player, key, value, oldValue, currentTime)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(
                "[EventCallback::PlayerOnStorageUpdate - Player {PlayerName} key {Key}] Call stack overflow. Too many lua script calls being nested.",
                player.Name, key);
            return;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        Lua.PushNumber(L, key);
        Lua.PushNumber(L, value);
        Lua.PushNumber(L, oldValue);
        Lua.PushNumber(L, currentTime);

        scriptInterface.CallVoidFunction(5);
    }

    public void PlayerOnThink(IPlayer player, int interval)
    {
        // onThink(player, interval)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(
                "[EventCallback::PlayerOnThink] player {PlayerName}. Call stack overflow. Too many lua script calls being nested.",
                player.Name);
            return;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = scriptInterface.GetLuaState();
        scriptInterface.PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        Lua.PushNumber(L, interval);

        scriptInterface.CallVoidFunction(2);
    }

    #endregion
}