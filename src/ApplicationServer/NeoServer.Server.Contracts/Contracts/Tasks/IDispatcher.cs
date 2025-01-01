using System.Threading;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IDispatcher
{
    long GlobalTime { get; }

    void AddEvent(IEvent evt);

    void Start(CancellationToken token);
}