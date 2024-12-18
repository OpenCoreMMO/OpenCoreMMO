using LuaNET;

namespace NeoServer.Scripts.LuaJIT;

public interface ILuaEnvironment : ILuaScriptInterface
{
    public LuaState GetLuaState();

    public bool InitState();

    public bool ReInitState();

    public bool CloseState();

    public LuaScriptInterface GetTestInterface();

    //public Combat GetCombatObject(uint id);

    //public Combat CreateCombatObject(LuaScriptInterface @interface);

    //public void ClearCombatObjects(LuaScriptInterface @interface);

    //public AreaCombat GetAreaObject(uint id);

    //public uint CreateAreaObject(LuaScriptInterface @interface);

    //public void ClearAreaObjects(LuaScriptInterface @interface);

    //public static T CreateWeaponObject<T>(LuaScriptInterface interfaceInstance) where T : YourWeaponType, new();

    //public static T GetWeaponObject<T>(uint id) where T : YourWeaponType;

    //public static void ClearWeaponObjects(LuaScriptInterface interfaceInstance);

    //public bool IsShuttingDown();

    public void ExecuteTimerEvent(uint eventIndex);

    public void CollectGarbage();
}
