using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Domain.Common;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;
using IEvent = NeoServer.Server.Common.Contracts.Tasks.IEvent;

namespace NeoServer.Server.Tasks;

public class Dispatcher : IDispatcher
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger _logger;
    private readonly ChannelReader<IEvent> _reader;
    private readonly ChannelWriter<IEvent> _writer;
    private Task _processingTask;

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

    public long GlobalTime { get; private set; }

    /// <summary>
    ///     Adds an event to dispatcher queue
    /// </summary>
    /// <param name="evt"></param>
    public void AddEvent(IEvent evt)
    {
        if (evt?.Action is null)
        {
            _logger.Warning("Dispatcher: Attempted to add null event or event with null action");
            return;
        }

        if (!_writer.TryWrite(evt))
        {
            _logger.Warning("Dispatcher: Failed to write event to channel - channel may be completed");
        }
    }

    /// <summary>
    ///     Starts dispatcher processing queue
    /// </summary>
    /// <param name="token"></param>
    public void Start(CancellationToken token)
    {
        if (_processingTask != null && !_processingTask.IsCompleted)
        {
            _logger.Warning("Dispatcher: Already started");
            return;
        }

        _processingTask = Task.Run(async () =>
        {
            try
            {
                while (await _reader.WaitToReadAsync(token).ConfigureAwait(false))
                {
                    GlobalTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    while (_reader.TryRead(out var evt))
                    {
                        if (evt?.Action is null)
                        {
                            _logger.Warning("Dispatcher: Skipping event with null action");
                            continue;
                        }

                        if (!evt.HasExpired || evt.HasNoTimeout)
                        {
                            try
                            {
                                evt.Action.Invoke();
                                _eventAggregator.PropagateEvents();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Dispatcher: Exception in event execution");
                            }
                        }
                        else
                        {
                            _logger.Debug("Dispatcher: Skipped expired event");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Information("Dispatcher: cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Dispatcher: fatal error");
                throw;
            }
            finally
            {
                try { _writer.Complete(); }
                catch (Exception ex) { _logger.Warning(ex, "Dispatcher: error completing channel"); }
            }
        }, CancellationToken.None);
    }

    // Additional method to wait for dispatcher completion (useful for tests and shutdown)
    public Task WaitForCompletionAsync()
    {
        return _processingTask ?? Task.CompletedTask;
    }
}