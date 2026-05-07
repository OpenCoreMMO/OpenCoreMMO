using System;
using System.Threading;

namespace NeoServer.Server.Common.Contracts.Tasks;

public interface IDispatcher : IDisposable
{
    long GlobalTime { get; }

    void AddEvent(IEvent evt);

    void Start(CancellationToken token);
    void WaitForCompletion();
}