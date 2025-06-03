using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;

namespace NeoServer.Server.Tasks;

public class OptimizedScheduler : Scheduler
{
    private readonly IDispatcher _dispatcher;
    protected readonly ConcurrentQueue<ISchedulerEvent> PreQueue = new();
    protected readonly object PreQueueMonitor = new();

    public OptimizedScheduler(IDispatcher dispatcher) : base(dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await Reader.WaitToReadAsync(token))
            while (Reader.TryRead(out var evt))
            {
                if (EventIsCancelled(evt.EventId))
                {
                    CancelledEventIds.TryRemove(evt.EventId, out _);
                    continue;
                }

                DispatchEvent(evt);
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        Task.Factory.StartNew(() =>
        {
            var replace = new List<ISchedulerEvent>();

            const int minDelay = 100;

            PreQueueLoop(minDelay, replace, token);
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private void PreQueueLoop(int minDelay, List<ISchedulerEvent> replace, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            lock (PreQueueMonitor)
            {
                Monitor.Wait(PreQueueMonitor, minDelay);
            }

            minDelay = 100;

            while (PreQueue.TryDequeue(out var evt))
            {
                var remainingTime = evt.RemainingTime;
                if (remainingTime > 0)
                {
                    minDelay = remainingTime < minDelay ? (int)remainingTime : minDelay;
                    replace.Add(evt);
                    continue;
                }

                AddEvent(evt);
            }

            foreach (var action in replace) PreQueue.Enqueue(action);

            replace.Clear();
        }
    }

    protected override bool DispatchEvent(ISchedulerEvent evt)
    {
        if (!evt.HasExpired)
        {
            ActiveEventIds.TryRemove(evt.EventId, out _);

            PreQueue.Enqueue(evt);
            lock (PreQueueMonitor)
            {
                Monitor.Pulse(PreQueueMonitor);
            }

            return false;
        }

        evt.SetToNotExpire();

        if (EventIsCancelled(evt.EventId))
        {
            CancelledEventIds.TryRemove(evt.EventId, out _);
            return false;
        }

        Interlocked.Increment(ref EventLength);
        ActiveEventIds.TryRemove(evt.EventId, out _);
        _dispatcher.AddEvent(evt); //send to dispatcher      

        return true;
    }
}