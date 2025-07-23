using LuaNET;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class ReloadManager (
    ILogger logger,
    ILuaEnvironment luaEnvironment,
    IScripts scripts,
    INpcs npcs,
    INpcTypeStore npcsTypeStore,
    ServerConfiguration serverConfiguration) : IReloadManager
{
    #region Private Members

    private int _isReloading; // Use int to Interlocked

    #endregion

    #region Public Methods

    public bool Reload(ReloadType reloadType)
    {
        // Try sets _isReloading to 1 (true) if 0 (false)
        if (Interlocked.CompareExchange(ref _isReloading, 1, 0) != 0)
            return false;

        var isReloaded = true;
        try
        {
            switch (reloadType)
            {
                case ReloadType.All:
                    {
                        ReloadCore();
                        ReloadScripts();
                        break;
                    }

                case ReloadType.Core:
                    {
                        ReloadCore();
                        break;
                    }

                case ReloadType.Npcs:
                    {
                        ReloadNpcs();
                        break;
                    }

                case ReloadType.Scripts:
                    {
                        ReloadScripts();
                        break;
                    }

                default:
                    isReloaded = false;
                    break;
            }

            Lua.GC(LuaEnvironment.GetInstance().GetLuaState(), LuaGCParam.Collect, 0);
        }
        catch (Exception e)
        {
            isReloaded = false;
            logger.Error(e, "[ReloadManager::Reload] - Error reloading scripts: {Message}", e.Message);
        }
        finally
        {
            Interlocked.Exchange(ref _isReloading, 0);
        }

        return isReloaded;
    }

    #endregion

    #region Private Methods
    private void ReloadCore()
    {
        var coreLoaded = luaEnvironment.LoadFile($"{serverConfiguration.Data}/core.lua", "core.lua");
        if (!coreLoaded) return;

        scripts.LoadScripts($"{serverConfiguration.Data}/scripts/libs", true, false);
    }

    private void ReloadNpcs()
    {
        npcs.Clear();
        npcsTypeStore.Clear();
        luaEnvironment.LoadFile($"{serverConfiguration.Data}/npclib/load.lua", "load.lua");
        scripts.LoadScripts($"{serverConfiguration.Data}/npcs", false, true);
    }

    private void ReloadScripts()
    {
        scripts.ClearAllScripts();
        scripts.LoadScripts($"{serverConfiguration.Data}/scripts", false, true);
        scripts.LoadScripts($"{serverConfiguration.Data}/scripts/libs", true, true);

        ReloadNpcs();
        //ReloadMonsters(dir);
    }
    #endregion

}