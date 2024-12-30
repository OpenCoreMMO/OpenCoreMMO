using System.ComponentModel;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;

namespace NeoServer.Scripts.LuaJIT;

public class ScriptEnvironment
{
    // local item map
    private readonly Dictionary<uint, IThing> localMap = new();

    // temporary item list
    private readonly Dictionary<ScriptEnvironment, IItem> tempItems = new();
    private int callbackId;

    // for npc scripts
    private INpc curNpc;
    private uint lastUID;

    private LuaScriptInterface luaScriptInterface;

    // script file id
    private int scriptId;
    private bool timerEvent;

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
        scriptId = 0;
        callbackId = 0;
        timerEvent = false;
        luaScriptInterface = null;
        localMap.Clear();
        tempItems.Clear();
    }

    public void SetScriptId(int newScriptId, LuaScriptInterface newScriptInterface)
    {
        scriptId = newScriptId;
        luaScriptInterface = newScriptInterface;
    }

    public bool SetCallbackId(int newCallbackId, LuaScriptInterface scriptInterface)
    {
        if (callbackId != 0)
        {
            // nested callbacks are not allowed
            if (luaScriptInterface != null) luaScriptInterface.ReportError("Nested callbacks!");
            return false;
        }

        callbackId = newCallbackId;
        luaScriptInterface = scriptInterface;
        return true;
    }

    public int GetScriptId()
    {
        return scriptId;
    }

    public LuaScriptInterface GetScriptInterface()
    {
        return luaScriptInterface;
    }

    public void SetTimerEvent()
    {
        timerEvent = true;
    }

    public void GetEventInfo(out int retScriptId, out LuaScriptInterface retScriptInterface, out int retCallbackId,
        out bool retTimerEvent)
    {
        retScriptId = scriptId;
        retScriptInterface = luaScriptInterface;
        retCallbackId = callbackId;
        retTimerEvent = timerEvent;
    }

    public uint AddThing(IThing thing)
    {
        if (thing == null) return 0;

        if (thing is ICreature creature) return creature.CreatureId;

        if (thing is IItem item && item.Metadata.Attributes.HasAttribute(ItemAttribute.UniqueId)) return item.UniqueId;

        foreach (var it in localMap)
            if (it.Value == thing)
                return it.Key;

        localMap[++lastUID] = thing;
        return lastUID;
    }

    public void InsertItem(uint uid, IItem item)
    {
        var result = localMap.TryAdd(uid, item);
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

        if (localMap.TryGetValue(uid, out var thing))
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

        localMap.Remove(uid, out _);
    }

    public void AddTempItem(IItem item)
    {
        tempItems.Add(this, item);
    }

    public void RemoveTempItem(IItem item)
    {
        foreach (var it in tempItems)
            if (it.Value == item)
            {
                tempItems.Remove(it.Key);
                break;
            }
    }

    public void SetNpc(INpc npc)
    {
        curNpc = npc;
    }

    public INpc GetNpc()
    {
        return curNpc;
    }
}