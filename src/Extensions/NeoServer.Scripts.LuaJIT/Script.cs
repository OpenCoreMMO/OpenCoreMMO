namespace NeoServer.Scripts.LuaJIT;

public class Script
{
    protected LuaScriptInterface _scriptInterface;

    // If script is loaded callback
    private bool LoadedCallback;

    private int ScriptId;

    /// <summary>
    ///     Explicit constructor.
    /// </summary>
    /// <param name="interface">Lua Script Interface</param>
    public Script(LuaScriptInterface scriptInterface)
    {
        _scriptInterface = scriptInterface;
    }

    /// <summary>
    ///     Check if script is loaded.
    /// </summary>
    /// <returns>true if loaded, false otherwise</returns>
    public bool IsLoadedCallback()
    {
        return LoadedCallback;
    }

    /// <summary>
    ///     Set the loaded callback status.
    /// </summary>
    /// <param name="loaded">Loaded status to set</param>
    public void SetLoadedCallback(bool loaded)
    {
        LoadedCallback = loaded;
    }

    // Load revscriptsys callback
    public bool LoadCallback()
    {
        if (_scriptInterface == null)
            //Logger.GetInstance().Error($"[Script.LoadCallback] ScriptInterface is null, scriptId = {ScriptId}");
            return false;

        if (ScriptId != 0)
            //Logger.GetInstance().Error($"[Script.LoadCallback] ScriptId is not zero, scriptId = {ScriptId}, scriptName {_scriptInterface.GetLoadingScriptName()}");
            return false;

        var id = _scriptInterface.GetEvent();
        if (id == -1)
            //Logger.GetInstance().Error($"[Script.LoadCallback] Event {GetScriptTypeName()} not found for script with name {_scriptInterface.GetLoadingScriptName()}");
            return false;

        SetLoadedCallback(true);
        ScriptId = id;
        return true;
    }

    // NOTE: Pure virtual method ( = 0) that must be implemented in derived classes
    // Script type (Action, CreatureEvent, GlobalEvent, MoveEvent, Spell, Weapon)
    public virtual string GetScriptTypeName()
    {
        return string.Empty;
    }

    // Method to access the ScriptInterface in derived classes
    public LuaScriptInterface GetScriptInterface()
    {
        return _scriptInterface;
    }

    public virtual void SetScriptInterface(LuaScriptInterface newInterface)
    {
        _scriptInterface = newInterface;
    }

    // Method to access the ScriptId in derived classes
    public virtual int GetScriptId()
    {
        return ScriptId;
    }

    public virtual void SetScriptId(int newScriptId)
    {
        ScriptId = newScriptId;
    }
}