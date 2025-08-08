using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;

namespace NeoServer.Server.Tasks;

public class OptimizedScheduler : Scheduler
{
    private readonly IDispatcher _dispatcher;
    private readonly ConcurrentQueue<ISchedulerEvent> _preQueue = new();
    private readonly SemaphoreSlim _preQueueSemaphore = new(0);
    private readonly CancellationTokenSource _internalCancellation = new();
    
    private volatile bool _isShuttingDown;

    public OptimizedScheduler(IDispatcher dispatcher) : base(dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override void Start(CancellationToken token)
    {
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCancellation.Token);

        // Main scheduler thread
        Task.Factory.StartNew(async () =>
        {
            try
            {
                await foreach (var evt in Reader.ReadAllAsync(combinedCts.Token))
                {
                    if (EventIsCancelled(evt.EventId))
                    {
                        CancelledEventIds.TryRemove(evt.EventId, out var _);
                        continue;
                    }

                    DispatchEvent(evt);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        // Pre-queue thread with improved timing
        Task.Factory.StartNew(async () =>
        {
            var replace = new List<ISchedulerEvent>();
            const int baseDelay = 50; // Reduced for better responsiveness

            try
            {
                while (!combinedCts.Token.IsCancellationRequested)
                {
                    var nextDelay = baseDelay;
                    
                    // Process all available events in pre-queue
                    while (_preQueue.TryDequeue(out var evt))
                    {
                        var remainingTime = evt.RemainingTime;
                        if (remainingTime > 0)
                        {
                            nextDelay = Math.Min(nextDelay, (int)Math.Max(remainingTime, 10));
                            replace.Add(evt);
                            continue;
                        }

                        AddEvent(evt);
                    }

                    // Re-add events that haven't expired yet
                    foreach (var action in replace) 
                        _preQueue.Enqueue(action);
                    
                    replace.Clear();

                    // Wait using semaphore or timeout
                    try
                    {
                        await _preQueueSemaphore.WaitAsync(nextDelay, combinedCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    protected override bool DispatchEvent(ISchedulerEvent evt)
    {
        if (!evt.HasExpired)
        {
            ActiveEventIds.TryRemove(evt.EventId, out _);

            _preQueue.Enqueue(evt);
            // Notify semaphore without blocking
            try
            {
                _preQueueSemaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // Ignore if already at maximum
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
        _dispatcher.AddEvent(evt);

        return true;
    }

    public override bool CancelEvent(uint eventId)
    {
        if (_isShuttingDown) return false;
        return base.CancelEvent(eventId);
    }

    public void Shutdown()
    {
        if (_isShuttingDown) return;
        
        _isShuttingDown = true;
        _internalCancellation.Cancel();
    }

    public void Dispose()
    {
        Shutdown();
        _preQueueSemaphore?.Dispose();
        _internalCancellation?.Dispose();
    }
}