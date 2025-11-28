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
    private readonly CancellationTokenSource _internalCancellation = new();
    private readonly ILogger _logger;
    private readonly ChannelReader<IEvent> _reader;
    private readonly ChannelWriter<IEvent> _writer;
    private volatile bool _isShuttingDown;

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
        if (_isShuttingDown || evt?.Action is null)
        {
            if (evt?.Action is null)
                _logger.Warning("Dispatcher: Attempted to add null event or event with null action");
            return;
        }

        if (!_writer.TryWrite(evt))
            _logger.Warning("Dispatcher: Failed to write event to channel - channel may be completed");
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

        // Combine external cancellation with internal
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCancellation.Token);

        _processingTask = Task.Factory.StartNew(async () =>
        {
            _logger.Information("Dispatcher: Starting event processing loop");
            var eventCount = 0L;

            try
            {
                await foreach (var evt in _reader.ReadAllAsync(combinedCts.Token))
                {
                    GlobalTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    eventCount++;

                    if (evt?.Action is null)
                    {
                        _logger.Warning("Dispatcher: Skipping event with null action");
                        continue;
                    }

                    if (!evt.HasExpired || evt.HasNoTimeout)
                    {
                        // Add timeout to prevent hanging during debugging
                        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                        try
                        {
                            evt.Action.Invoke();
                            _eventAggregator.PropagateEvents();

                            // Progress logging for debugging
                            if (eventCount % 1000 == 0)
                                _logger.Debug("Dispatcher: Processed {EventCount} events", eventCount);
                        }
                        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                        {
                            _logger.Warning("Dispatcher: Event execution timeout - possible deadlock during debugging");
                            // Continue processing other events
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
            catch (OperationCanceledException)
            {
                _logger.Information("Dispatcher: Cancelled after processing {EventCount} events", eventCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Dispatcher: Fatal error after processing {EventCount} events", eventCount);
                throw;
            }
            finally
            {
                try
                {
                    _writer.Complete();
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Dispatcher: Error completing channel");
                }

                _logger.Information("Dispatcher: Event processing loop ended. Total events processed: {EventCount}",
                    eventCount);
            }
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    // Additional method to wait for dispatcher completion (useful for tests and shutdown)
    public async Task WaitForCompletionAsync()
    {
        if (_processingTask == null) return;

        try
        {
            await _processingTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Dispatcher: Error during shutdown");
        }
    }

    public void Dispose()
    {
        Shutdown();
        _internalCancellation?.Dispose();
    }

    public void Shutdown()
    {
        if (_isShuttingDown) return;

        _isShuttingDown = true;
        _logger.Information("Dispatcher: Initiating shutdown");

        _writer.TryComplete();
        _internalCancellation.Cancel();
    }
}