using System;
using System.Threading;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IPersistenceDispatcher : IDisposable
{
    void AddEvent(Action evt);
    void Start(CancellationToken token);
    void WaitForCompletion();
    void Shutdown();
}