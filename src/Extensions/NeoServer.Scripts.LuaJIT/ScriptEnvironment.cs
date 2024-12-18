using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeoServer.Scripts.LuaJIT;

public class ScriptEnvironment
{
    // script file id
    private int scriptId;
    private int callbackId;
    private bool timerEvent;

    private LuaScriptInterface luaScriptInterface;

    // local item map
    //private Dictionary<uint, std.shared_ptr<Thing>> localMap;
    //private uint lastUID;

    // temporary item list
    //private Dictionary<ScriptEnvironment, std.shared_ptr<Item>> tempItems;

    // for npc scripts
    //private Npc curNpc = nullptr;

    // result map
    //private uint lastResultId;
    //private Dictionary<uint, DBResult_ptr> tempResults;

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
        //localMap.Clear();
        //tempResults.Clear();

        //var pair = tempItems.equal_range(this);
        //var it = pair.first;
        //while (it != pair.second)
        //{
        //    std.shared_ptr<Item> item = it->second;
        //    it = tempItems.erase(it);
        //}
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
            if (luaScriptInterface != null)
            {
                luaScriptInterface.ReportError("Nested callbacks!");
            }
            return false;
        }

        callbackId = newCallbackId;
        luaScriptInterface = scriptInterface;
        return true;
    }

    public int GetScriptId() {
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

    public void GetEventInfo(out int retScriptId, out LuaScriptInterface retScriptInterface, out int retCallbackId, out bool retTimerEvent)
    {
        retScriptId = scriptId;
        retScriptInterface = luaScriptInterface;
        retCallbackId = callbackId;
        retTimerEvent = timerEvent;
    }

    //public uint AddThing(std.shared_ptr<Thing> thing)
    //{
    //    if (thing == null || thing.isRemoved())
    //    {
    //        return 0;
    //    }

    //    std.shared_ptr<Creature> creature = thing.getCreature();
    //    if (creature != null)
    //    {
    //        return creature.getID();
    //    }

    //    std.shared_ptr<Item> item = thing.getItem();
    //    if (item != null && item.hasAttribute(ItemAttribute_t.UNIQUEID))
    //    {
    //        return item.getAttribute<uint>(ItemAttribute_t.UNIQUEID);
    //    }

    //    foreach (var it in localMap)
    //    {
    //        if (it.Value == item)
    //        {
    //            return it.Key;
    //        }
    //    }

    //    localMap[++lastUID] = item;
    //    return lastUID;
    //}

    //public void InsertItem(uint uid, std.shared_ptr<Item> item)
    //{
    //    var result = localMap.TryAdd(uid, item);
    //    if (!result)
    //    {
    //        g_logger().error("Thing uid already taken: {}", uid);
    //    }
    //}

    //public std.shared_ptr<Thing> GetThingByUID(uint uid)
    //{
    //    if (uid >= 0x10000000)
    //    {
    //        return g_game().getCreatureByID(uid);
    //    }

    //    if (uid <= std.numeric_limits<uint16_t>.max())
    //    {
    //        std.shared_ptr<Item> item = g_game().getUniqueItem(static_cast<uint16_t>(uid));
    //        if (item != null && !item.isRemoved())
    //        {
    //            return item;
    //        }
    //        return null;
    //    }

    //    if (localMap.TryGetValue(uid, out std.shared_ptr<Item> item))
    //    {
    //        if (!item.isRemoved())
    //        {
    //            return item;
    //        }
    //    }
    //    return null;
    //}

    //public std.shared_ptr<Item> GetItemByUID(uint uid)
    //{
    //    std.shared_ptr<Thing> thing = getThingByUID(uid);
    //    if (thing == null)
    //    {
    //        return null;
    //    }
    //    return thing.getItem();
    //}

    //public std.shared_ptr<Container> GetContainerByUID(uint uid)
    //{
    //    std.shared_ptr<Item> item = getItemByUID(uid);
    //    if (item == null)
    //    {
    //        return null;
    //    }
    //    return item.getContainer();
    //}

    //public void RemoveItemByUID(uint uid)
    //{
    //    if (uid <= std.numeric_limits<uint16_t>.max())
    //    {
    //        g_game().removeUniqueItem(static_cast<uint16_t>(uid));
    //        return;
    //    }

    //    localMap.TryRemove(uid, out var _);
    //}

    //public void AddTempItem(std.shared_ptr<Item> item)
    //{
    //    tempItems.Add(this, item);
    //}

    //public void RemoveTempItem(std.shared_ptr<Item> item)
    //{
    //    foreach (var it in tempItems)
    //    {
    //        if (it.Value == item)
    //        {
    //            tempItems.Remove(it.Key);
    //            break;
    //        }
    //    }
    //}

    //public uint AddResult(DBResult_ptr res)
    //{
    //    tempResults[++lastResultId] = res;
    //    return lastResultId;
    //}

    //public bool RemoveResult(uint id)
    //{
    //    if (tempResults.TryGetValue(id, out var _))
    //    {
    //        tempResults.Remove(id);
    //        return true;
    //    }
    //    return false;
    //}

    //public DBResult_ptr GetResultByID(uint id)
    //{
    //    if (tempResults.TryGetValue(id, out var result))
    //    {
    //        return result;
    //    }
    //    return null;
    //}

 //   public void SetNpc(Npc npc)
 //   {
 //       curNpc = npc;
 //   }

 //   public Npc GetNpc() {
		   // return curNpc;
	    //}
}
