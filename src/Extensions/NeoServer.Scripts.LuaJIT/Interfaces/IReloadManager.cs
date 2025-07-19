using NeoServer.Domain.Common;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface IReloadManager
{
    bool Reload(ReloadType reloadType);
}