using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;

namespace NeoServer.Server.Tasks;

public class Scheduler : IScheduler
{
    private readonly IDispatcher _dispatcher;
    private readonly ChannelWriter<ISchedulerEvent> _writer;

    protected readonly ConcurrentDictionary<uint, byte> ActiveEventIds = new();
    protected readonly ConcurrentDictionary<uint, byte> CancelledEventIds = new();
    protected readonly ChannelReader<ISchedulerEvent> Reader;

    private uint _lastEventId;
    protected ulong EventLength;

    protected Scheduler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        var channel = Channel.CreateUnbounded<ISchedulerEvent>(new UnboundedChannelOptions { SingleReader = true });
        Reader = channel.Reader;
        _writer = channel.Writer;
    }

    public ulong Count => EventLength;
    public bool Empty => ActiveEventIds.IsEmpty;
    public long GlobalTime => _dispatcher.GlobalTime;

    /// <summary>
    ///     Adds event to be scheduled on the queue
    /// </summary>
    /// <param name="evt"></param>
    /// <returns></returns>
    public virtual uint AddEvent(ISchedulerEvent evt)
    {
        if (evt.EventId == default) evt.SetEventId(++_lastEventId);

        if (ActiveEventIds.TryAdd(evt.EventId, default))
            _writer.TryWrite(evt);

        return evt.EventId;
    }

    /// <summary>
    ///     Starts scheduler queue
    /// </summary>
    /// <param name="token"></param>
    public virtual void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                await foreach (var evt in Reader.ReadAllAsync(token))
                {
                    if (EventIsCancelled(evt.EventId)) continue;

                    if (!evt.HasExpired)
                    {
                        // Use Task.Delay instead of ThreadPool for better debugging
                        _ = Task.Delay(evt.ExpirationDelay, token)
                            .ContinueWith(task =>
                            {
                                if (!token.IsCancellationRequested)
                                {
                                    ActiveEventIds.TryRemove(evt.EventId, out _);
                                    AddEvent(evt);
                                }
                            }, TaskScheduler.Default);
                        continue;
                    }

                    DispatchEvent(evt);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>
    ///     Cancels event. Event will be not dispatched
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public virtual bool CancelEvent(uint eventId)
    {
        if (eventId == default) return false;
        var removed = ActiveEventIds.TryRemove(eventId, out _);
        CancelledEventIds.TryAdd(eventId, default);
        return removed;
    }

    /// <summary>
    ///     Indicates whether event was cancelled
    /// </summary>
    /// <param name="eventId"></param>
    /// <returns></returns>
    public bool EventIsCancelled(uint eventId)
    {
        return CancelledEventIds.ContainsKey(eventId);
    }

    protected virtual bool DispatchEvent(ISchedulerEvent evt)
    {
        evt.SetToNotExpire();

        if (!EventIsCancelled(evt.EventId))
        {
            Interlocked.Increment(ref EventLength);
            ActiveEventIds.TryRemove(evt.EventId, out _);
            _dispatcher.AddEvent(evt);
            return true;
        }

        return false;
    }
}