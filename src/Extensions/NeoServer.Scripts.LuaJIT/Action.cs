using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Action(LuaScriptInterface scriptInterface) : Script(scriptInterface)
{
    private ILogger _logger;

    public bool AllowFarUse { get; set; }
    public bool CheckLineOfSight { get; set; }

    public bool CheckFloor { get; set; }

    public List<ushort> ItemIdsVector { get; } = [];

    public List<uint> UniqueIdsVector { get; } = [];

    public List<ushort> ActionIdsVector { get; } = [];

    public List<Location> PositionsVector { get; } = [];

    public bool ExecuteUse(IPlayer player, IItem item, Location fromPosition, IThing? target, Location toPosition,
        bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger ??= Server.Helpers.IoC.GetInstance<ILogger>();

            _logger.Error("[Action::executeUse - Player {PlayerName}, on item {ItemName}]. Call stack overflow. Too many lua script calls being nested. Script name {ScriptName}",
                player.Name, item.Name, GetScriptInterface().GetLoadingScriptName());
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var luaState = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(luaState, player);
        LuaScriptInterface.SetMetatable(luaState, -1, "Player");

        LuaScriptInterface.PushThing(luaState, item);
        LuaScriptInterface.PushPosition(luaState, fromPosition);

        LuaScriptInterface.PushThing(luaState, target);
        LuaScriptInterface.PushPosition(luaState, toPosition);

        LuaScriptInterface.PushBoolean(luaState, isHotkey);

        return GetScriptInterface().CallFunction(6);
    }

    public void SetItemIdsVector(ushort id) => ItemIdsVector.Add(id);

    public void SetUniqueIdsVector(ushort id) => UniqueIdsVector.Add(id);

    public void SetActionIdsVector(ushort id) => ActionIdsVector.Add(id);

    public void SetPositionsVector(Location pos) => PositionsVector.Add(pos);

    public bool HasPosition(Location position) => PositionsVector.Exists(p => p.Equals(position));

    public List<Location> GetPositions() => PositionsVector;

    public void SetPositions(Location pos) => PositionsVector.Add(pos);

    public virtual ReturnValueType CanExecuteAction(IPlayer player, Location toPos)
    {
        //todo: implement this?
        //if (!allowFarUse)
        //{
        //    return g_actions().canUse(player, toPos);
        //}

        //return g_actions().canUseFar(player, toPos, checkLineOfSight, checkFloor);

        return default;
    }
}