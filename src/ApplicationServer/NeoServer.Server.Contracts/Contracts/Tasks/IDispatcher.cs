using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IDispatcher : IDisposable
{
    long GlobalTime { get; }

    void AddEvent(IEvent evt);

    void Start(CancellationToken token);
    Task WaitForCompletionAsync();
}