using System;
using Moq;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Server;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Managers;
using NeoServer.Server.Tasks;
using Serilog;

namespace NeoServer.Game.Tests.Server;

public static class GameServerTestBuilder
{
    public static IGameServer Build(IMap map)
    {
        var logger = new Mock<ILogger>().Object;
        var eventAggregator = new Mock<IEventAggregator>().Object;
        var dispatcher = new Dispatcher(logger, eventAggregator);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(Array.Empty<IItemType>());
        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, itemTypeStore);
        var persistenceDispatcher = new PersistenceDispatcher(logger);

        var worldRecordRepository = new Mock<IWorldRecordRepository>().Object;

        var gameServer = new GameServer(map, dispatcher, new OptimizedScheduler(dispatcher),
            new GameCreatureManager(
                new Mock<ICreatureGameInstance>().Object, map, logger, worldRecordRepository),
            decayableItemManager,
            persistenceDispatcher);

        return gameServer;
    }
}