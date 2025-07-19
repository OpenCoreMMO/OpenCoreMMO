using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Domain.Tests.Server;

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
            new Mock<ITalkActionScriptService>().Object,
            new Mock<IReloadManager>().Object,
            CreateMockServerConfiguration()
        );
    }

    public static ServerConfiguration CreateMockServerConfiguration()
    {
        return new ServerConfiguration(
            Version: 1,
            OTBM: "mock.otbm",
            OTB: "mock.otb",
            Data: "/mock/data",
            ServerName: "MockServer",
            ServerIp: "127.0.0.1",
            Extensions: "/mock/extensions",
            ServerLoginPort: 7171,
            ServerGamePort: 7172,
            AutoReloadScripts: true,
            Save: new SaveConfiguration(Players: 100)
        );
    }
}