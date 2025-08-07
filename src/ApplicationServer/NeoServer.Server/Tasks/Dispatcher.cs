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

        _logger.Information("Dispatcher: Starting event processing");

        _processingTask = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Use timeout on WaitToReadAsync to allow periodic cancellation checks
                    var waitTask = _reader.WaitToReadAsync(CancellationToken.None).AsTask();
                    var delayTask = Task.Delay(100, token); // 100ms timeout

                    var completedTask = await Task.WhenAny(waitTask, delayTask);

                    if (completedTask == delayTask)
                    {
                        // Timeout - continue loop to check for cancellation
                        if (token.IsCancellationRequested)
                            break;
                        continue;
                    }

                    if (!await waitTask)
                    {
                        // Channel was completed
                        _logger.Information("Dispatcher: Channel was completed, exiting");
                        break;
                    }

                    // Update GlobalTime once per iteration
                    GlobalTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    // Process all available events
                    var eventsProcessed = 0;
                    while (_reader.TryRead(out var evt) && !token.IsCancellationRequested)
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
                                eventsProcessed++;

                                _logger.Verbose("Dispatcher: Executed event {Action}", evt.Action.Target?.ToString());
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

                    if (eventsProcessed > 0)
                    {
                        _logger.Debug("Dispatcher: Processed {Count} events in this iteration", eventsProcessed);
                    }
                }

                _logger.Information("Dispatcher: Processing loop ended due to cancellation");
            }
            catch (OperationCanceledException)
            {
                _logger.Information("Dispatcher: Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Dispatcher: Fatal exception occurred, dispatcher will stop");
                throw; // Re-throw so it can be detected as a fatal failure
            }
            finally
            {
                // Only close the channel when we are actually exiting
                try
                {
                    _writer.Complete();
                    _logger.Information("Dispatcher: Channel completed");
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Dispatcher: Error completing channel");
                }
            }
        }, CancellationToken.None); // Don't pass the token here to avoid automatic cancellation

        _logger.Information("Dispatcher: Started successfully");
    }

    // Additional method to wait for dispatcher completion (useful for tests and shutdown)
    public Task WaitForCompletionAsync()
    {
        return _processingTask ?? Task.CompletedTask;
    }
}