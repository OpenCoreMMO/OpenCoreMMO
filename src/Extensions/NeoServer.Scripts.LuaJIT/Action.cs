using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Action : Script
{
    private ILogger _logger;

    public Action(LuaScriptInterface scriptInterface) : base(scriptInterface)
    {
    }

    public bool AllowFarUse { get; set; }
    public bool CheckLineOfSight { get; set; }

    public bool CheckFloor { get; set; }

    public List<ushort> ItemIdsVector { get; } = new();

    public List<uint> UniqueIdsVector { get; } = new();

    public List<ushort> ActionIdsVector { get; } = new();

    public List<Location> PositionsVector { get; } = new();

    public bool ExecuteUse(IPlayer player, IItem item, Location fromPosition, IThing? target, Location toPosition,
        bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            if (_logger == null)
                _logger = Server.Helpers.IoC.GetInstance<ILogger>();

            _logger.Error($"[Action::executeUse - Player {player.Name}, on item {item.Name}] " +
                          $"Call stack overflow. Too many lua script calls being nested. Script name {GetScriptInterface().GetLoadingScriptName()}");
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

    public void SetItemIdsVector(ushort id)
    {
        ItemIdsVector.Add(id);
    }

    public void SetUniqueIdsVector(ushort id)
    {
        UniqueIdsVector.Add(id);
    }

    public void SetActionIdsVector(ushort id)
    {
        ActionIdsVector.Add(id);
    }

    public void SetPositionsVector(Location pos)
    {
        PositionsVector.Add(pos);
    }

    public bool HasPosition(Location position)
    {
        return PositionsVector.Exists(p => p.Equals(position));
    }

    public List<Location> GetPositions()
    {
        return PositionsVector;
    }

    public void SetPositions(Location pos)
    {
        PositionsVector.Add(pos);
    }

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