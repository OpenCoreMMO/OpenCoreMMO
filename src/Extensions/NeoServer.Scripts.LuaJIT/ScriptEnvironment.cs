using System.ComponentModel;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;

namespace NeoServer.Scripts.LuaJIT;

public class ScriptEnvironment
{
    // local item map
    private readonly Dictionary<uint, IThing> _localMap = new();

    // temporary item list
    private readonly Dictionary<ScriptEnvironment, IItem> _tempItems = new();
    private int _callbackId;

    // for npc scripts
    private INpc _curNpc;
    private uint _lastUID;

    private LuaScriptInterface _luaScriptInterface;

    // script file id
    private int _scriptId;
    private bool _timerEvent;

    // result map
    static uint _lastResultId;
    private readonly Dictionary<uint, DBResult> _tempResults = new();

    public ScriptEnvironment()
    {
        ResetEnv();
    }

    ~ScriptEnvironment()
    {
        ResetEnv();
    }

    public void ResetEnv()
    {
        _scriptId = 0;
        _callbackId = 0;
        _timerEvent = false;
        _luaScriptInterface = null;
        _localMap.Clear();
        _tempItems.Clear();
    }

    public void SetScriptId(int newScriptId, LuaScriptInterface newScriptInterface)
    {
        _scriptId = newScriptId;
        _luaScriptInterface = newScriptInterface;
    }

    public bool SetCallbackId(int newCallbackId, LuaScriptInterface scriptInterface)
    {
        if (_callbackId != 0)
        {
            // nested callbacks are not allowed
            if (_luaScriptInterface != null) _luaScriptInterface.ReportError("Nested callbacks!");
            return false;
        }

        _callbackId = newCallbackId;
        _luaScriptInterface = scriptInterface;
        return true;
    }

    public int GetScriptId()
    {
        return _scriptId;
    }

    public LuaScriptInterface GetScriptInterface()
    {
        return _luaScriptInterface;
    }

    public void SetTimerEvent()
    {
        _timerEvent = true;
    }

    public void GetEventInfo(out int retScriptId, out LuaScriptInterface retScriptInterface, out int retCallbackId,
        out bool retTimerEvent)
    {
        retScriptId = _scriptId;
        retScriptInterface = _luaScriptInterface;
        retCallbackId = _callbackId;
        retTimerEvent = _timerEvent;
    }

    public uint AddThing(IThing thing)
    {
        if (thing == null) return 0;

        if (thing is ICreature creature) return creature.CreatureId;

        if (thing is IItem item && item.Metadata.Attributes.HasAttribute(ItemAttribute.UniqueId)) return item.UniqueId;

        foreach (var it in _localMap)
            if (it.Value == thing)
                return it.Key;

        _localMap[++_lastUID] = thing;
        return _lastUID;
    }

    public void InsertItem(uint uid, IItem item)
    {
        var result = _localMap.TryAdd(uid, item);
        if (!result)
        {
            //todo: implement this
            //g_logger().error("Thing uid already taken: {}", uid);
        }
    }

    public IThing GetThingByUID(uint uid)
    {
        //todo: implement this
        //if (uid >= 0x10000000)
        //{
        //    return g_game().getCreatureByID(uid);
        //}

        if (uid <= ushort.MaxValue)
        {
            //std.shared_ptr<Item> item = g_game().getUniqueItem(static_cast<uint16_t>(uid));
            //if (item != null && !item.isRemoved())
            //{
            //    return item;
            //}
            //return null;
        }

        if (_localMap.TryGetValue(uid, out var thing))
            return thing;

        return null;
    }

    public IItem GetItemByUID(uint uid)
    {
        var thing = GetThingByUID(uid);

        if (thing != null && thing is IItem item)
            return item;

        return null;
    }

    public IContainer GetContainerByUID(uint uid)
    {
        var thing = GetThingByUID(uid);

        if (thing != null && thing is IContainer container)
            return container;

        return null;
    }

    public void RemoveItemByUID(uint uid)
    {
        if (uid <= ushort.MaxValue)
            //g_game().removeUniqueItem(static_cast<uint16_t>(uid));
            return;

        _localMap.Remove(uid, out _);
    }

    public void AddTempItem(IItem item)
    {
        _tempItems.Add(this, item);
    }

    public void RemoveTempItem(IItem item)
    {
        foreach (var it in _tempItems)
            if (it.Value == item)
            {
                _tempItems.Remove(it.Key);
                break;
            }
    }

    public void SetNpc(INpc npc)
    {
        _curNpc = npc;
    }

    public INpc GetNpc()
    {
        return _curNpc;
    }

    public DBResult GetResultByID(uint id)
        => _tempResults.GetValueOrDefault(id);

    public uint AddResult(DBResult res)
    {
        _tempResults.Add(++_lastResultId, res);
        return _lastResultId;
    }

    public bool RemoveResult(uint id)
        => _tempResults.Remove(id);
}