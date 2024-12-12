using Moq;
using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Dispatcher;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Modules.Creatures;
using Serilog;

namespace NeoServer.Game.Tests.Server;

public static class GameServerTestBuilder
{
    public static IGameServer Build(IMap map)
    {
        var logger = new Mock<ILogger>().Object;
        var dispatcher = new Dispatcher(logger);

        var persistenceDispatcher = new PersistenceDispatcher(logger);

        var gameServer = new GameServer(
            map,
            dispatcher,
            new OptimizedScheduler(dispatcher),
            new GameCreatureManager(new Mock<ICreatureGameInstance>().Object, map, logger),
            persistenceDispatcher);

        return gameServer;
    }
}