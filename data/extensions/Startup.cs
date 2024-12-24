using NeoServer.Server.Services;
using NeoServer.Server.Common.Contracts;
using NLua;

namespace NeoServer.Extensions;

public class Startup : IStartup
{
    private readonly Lua _lua;

    public Startup(Lua lua)
    {
        _lua = lua;
    }

    public void Run()
    {
        _lua["sendEffect"] = EffectService.Send;
    }
}