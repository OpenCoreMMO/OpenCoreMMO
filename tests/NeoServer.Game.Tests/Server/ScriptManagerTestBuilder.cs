using Moq;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Game.Tests.Server;

public static class ScriptManagerTestBuilder
{
    public static IScriptManager Build(params IItem[] items)
    {
        return new LuaScriptManager(
            new Mock<ILuaStartup>().Object,
            new Mock<IGlobalEvents>().Object,
            new Mock<ILogger>().Object,
            new Mock<IActionScriptService>().Object,
            new Mock<ICreatureEventsScriptService>().Object,
            new Mock<IGlobalEventsScriptService>().Object,
            new Mock<IMoveEventsScriptService>().Object,
            new Mock<ITalkActionScriptService>().Object
        );
    }
}