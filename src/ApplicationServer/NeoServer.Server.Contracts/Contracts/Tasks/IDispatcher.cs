using System.Threading;
using System.Threading.Tasks;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IDispatcher
{
    long GlobalTime { get; }

    void AddEvent(IEvent evt);

    void Start(CancellationToken token);
    Task WaitForCompletionAsync();
}