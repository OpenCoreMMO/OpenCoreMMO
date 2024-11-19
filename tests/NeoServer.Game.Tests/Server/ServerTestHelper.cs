using System.Threading;
using NeoServer.BuildingBlocks.Application.Contracts;

namespace NeoServer.Game.Tests.Server;

public class ServerTestHelper
{
    public static CancellationToken StartThreads(IGameServer gameServer)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        gameServer.Dispatcher.Start(cancellationToken);
        gameServer.Scheduler.Start(cancellationToken);

        return cancellationToken;
    }
}