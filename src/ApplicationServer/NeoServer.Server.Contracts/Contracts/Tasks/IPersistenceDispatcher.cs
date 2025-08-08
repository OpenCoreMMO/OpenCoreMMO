using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IPersistenceDispatcher : IDisposable
{
    void AddEvent(Func<Task> evt);
    void Start(CancellationToken token);
    Task WaitForCompletionAsync();
    void Shutdown();
}