using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Game.Common;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;
using IEvent = NeoServer.Server.Common.Contracts.Tasks.IEvent;

namespace NeoServer.Server.Tasks;

public class Dispatcher : IDispatcher
{
    private readonly ILogger _logger;
    private readonly IEventAggregator _eventAggregator;
    private readonly ChannelReader<IEvent> _reader;
    private readonly ChannelWriter<IEvent> _writer;

    public long GlobalTime { get; private set; }

    /// <summary>
    ///     A queue responsible for process events
    /// </summary>
    public Dispatcher(ILogger logger, IEventAggregator eventAggregator)
    {
        var channel = Channel.CreateUnbounded<IEvent>(new UnboundedChannelOptions { SingleReader = true });
        _reader = channel.Reader;
        _writer = channel.Writer;
        _logger = logger;
        _eventAggregator = eventAggregator;
    }

    /// <summary>
    ///     Adds an event to dispatcher queue
    /// </summary>
    /// <param name="evt"></param>
    public void AddEvent(IEvent evt)
    {
        _writer.TryWrite(evt);
    }

    /// <summary>
    ///     Starts dispatcher processing queue
    /// </summary>
    /// <param name="token"></param>
    public void Start(CancellationToken token)
    {
        Task.Factory.StartNew(async () =>
        {
            while (await _reader.WaitToReadAsync(token))

            {
                GlobalTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

                if (token.IsCancellationRequested) _writer.Complete();
                // Fast loop around available jobs
                while (_reader.TryRead(out var evt))
                {
                    if (!evt.HasExpired || evt.HasNoTimeout)
                        try
                        {
                            evt.Action?.Invoke(); //execute event
                            _eventAggregator.PropagateEvents(); //propagate events
                            
                            _logger.Verbose(evt.Action?.Target?.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Game event exception");
                        }
                }
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
}