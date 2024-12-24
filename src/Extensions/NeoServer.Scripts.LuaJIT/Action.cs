using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Server.Helpers;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Action : Script
{
    private bool _allowFarUse = false;
    private bool _checkFloor = true;
    private bool _checkLineOfSight = true;

    private List<ushort> _itemIds = new List<ushort>();
    private List<ushort> _uniqueIds = new List<ushort>();
    private List<ushort> _actionIds = new List<ushort>();
    private List<Location> _positions = new List<Location>();

    private ILogger _logger;

    public Action(LuaScriptInterface scriptInterface) : base(scriptInterface)
    {
    }

    public bool ExecuteUse(IPlayer player, IItem item, Location fromPosition, IThing target, Location toPosition, bool isHotkey)
    {
        // onUse(player, item, fromPosition, target, toPosition, isHotkey)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            if (_logger == null)
                _logger = IoC.GetInstance<ILogger>();

            _logger.Error($"[Action::executeUse - Player {player.Name}, on item {item.Name}] " +
                              $"Call stack overflow. Too many lua script calls being nested. Script name {GetScriptInterface().GetLoadingScriptName()}");
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");

        LuaScriptInterface.PushThing(L, item);
        LuaScriptInterface.PushPosition(L, fromPosition);

        LuaScriptInterface.PushThing(L, target);
        LuaScriptInterface.PushPosition(L, toPosition);

        LuaScriptInterface.PushBoolean(L, isHotkey);

        return GetScriptInterface().CallFunction(6);
    }

    public bool AllowFarUse
    {
        get { return _allowFarUse; }
        set { _allowFarUse = value; }
    }

    public bool CheckLineOfSight
    {
        get { return _checkLineOfSight; }
        set { _checkLineOfSight = value; }
    }

    public bool CheckFloor
    {
        get { return _checkFloor; }
        set { _checkFloor = value; }
    }

    public List<ushort> ItemIdsVector
    {
        get { return _itemIds; }
    }

    public void SetItemIdsVector(ushort id)
    {
        _itemIds.Add(id);
    }

    public List<ushort> UniqueIdsVector
    {
        get { return _uniqueIds; }
    }

    public void SetUniqueIdsVector(ushort id)
    {
        _uniqueIds.Add(id);
    }

    public List<ushort> ActionIdsVector
    {
        get { return _actionIds; }
    }

    public void SetActionIdsVector(ushort id)
    {
        _actionIds.Add(id);
    }

    public List<Location> PositionsVector
    {
        get { return _positions; }
    }

    public void SetPositionsVector(Location pos)
    {
        _positions.Add(pos);
    }

    public bool HasPosition(Location position)
    {
        return _positions.Exists(p => p.Equals(position));
    }

    public List<Location> GetPositions()
    {
        return _positions;
    }

    public void SetPositions(Location pos)
    {
        _positions.Add(pos);
    }

    public virtual ReturnValueType CanExecuteAction(IPlayer player, Location toPos)
    {
        //todo: implement this?
        //if (!allowFarUse)
        //{
        //    return g_actions().canUse(player, toPos);
        //}

        //return g_actions().canUseFar(player, toPos, checkLineOfSight, checkFloor);

        return default(ReturnValueType);
    }
}