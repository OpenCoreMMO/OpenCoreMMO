using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;

namespace NeoServer.Server.Tasks;

public class PersistenceDispatcher : IPersistenceDispatcher
{
    private readonly CancellationTokenSource _internalCancellation = new();
    private readonly ILogger _logger;
    private readonly ChannelReader<Func<Task>> _reader;
    private readonly ChannelWriter<Func<Task>> _writer;
    private volatile bool _isShuttingDown;

    private Task _processingTask;

    /// <summary>
    ///     A queue responsible for processing persistence events
    /// </summary>
    public PersistenceDispatcher(ILogger logger)
    {
        var channel = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions { SingleReader = true });
        _reader = channel.Reader;
        _writer = channel.Writer;
        _logger = logger;
    }

    /// <summary>
    ///     Adds a persistence event to the dispatcher queue
    /// </summary>
    /// <param name="evt"></param>
    public void AddEvent(Func<Task> evt)
    {
        if (_isShuttingDown || evt is null)
        {
            if (evt is null)
                _logger.Warning("PersistenceDispatcher: Attempted to add null event");
            return;
        }

        if (!_writer.TryWrite(evt))
            _logger.Warning("PersistenceDispatcher: Failed to write event to channel - channel may be completed");
    }

    /// <summary>
    ///     Starts persistence dispatcher processing queue
    /// </summary>
    /// <param name="token"></param>
    public void Start(CancellationToken token)
    {
        if (_processingTask != null && !_processingTask.IsCompleted)
        {
            _logger.Warning("PersistenceDispatcher: Already started");
            return;
        }

        // Combine external cancellation with internal
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCancellation.Token);

        _logger.Debug("PersistenceDispatcher: Starting persistence processing loop");

        _processingTask = Task.Factory.StartNew(async () =>
        {
            var eventCount = 0L;

            try
            {
                await foreach (var evt in _reader.ReadAllAsync(combinedCts.Token))
                {
                    eventCount++;

                    // Add timeout to prevent hanging during debugging
                    using var timeoutCts =
                        new CancellationTokenSource(TimeSpan.FromMinutes(5)); // Longer timeout for DB operations

                    try
                    {
                        var persistenceTask = Task.Run(async () => await evt().ConfigureAwait(false), timeoutCts.Token);

                        await persistenceTask;

                        // Progress logging for debugging
                        if (eventCount % 100 == 0)
                            _logger.Debug("PersistenceDispatcher: Processed {EventCount} persistence events",
                                eventCount);
                    }
                    catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                    {
                        _logger.Warning(
                            "PersistenceDispatcher: Persistence operation timeout - possible database deadlock during debugging");
                        // Continue processing other events
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "PersistenceDispatcher: Error during persistence operation");
                        // Continue processing other events instead of crashing
                    }
                }

                _logger.Debug(
                    "PersistenceDispatcher: Channel completed, stopping processing after {EventCount} events",
                    eventCount);
            }
            catch (OperationCanceledException)
            {
                _logger.Error("PersistenceDispatcher: Cancelled after processing {EventCount} events",
                    eventCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PersistenceDispatcher: Fatal exception after processing {EventCount} events",
                    eventCount);
                throw;
            }
            finally
            {
                try
                {
                    _writer.Complete();
                    _logger.Debug("PersistenceDispatcher: Channel completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "PersistenceDispatcher: Error completing channel");
                }

                _logger.Debug(
                    "PersistenceDispatcher: Processing loop ended. Total persistence events processed: {EventCount}",
                    eventCount);
            }
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        _logger.Debug("PersistenceDispatcher: Started successfully");
    }

    /// <summary>
    ///     Wait for dispatcher completion (useful for tests and shutdown)
    /// </summary>
    public async Task WaitForCompletionAsync()
    {
        if (_processingTask == null) return;

        try
        {
            await _processingTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "PersistenceDispatcher: Error during shutdown");
        }
    }

    /// <summary>
    ///     Initiate graceful shutdown of the persistence dispatcher
    /// </summary>
    public void Shutdown()
    {
        if (_isShuttingDown) return;

        _isShuttingDown = true;
        _logger.Information("PersistenceDispatcher: Initiating shutdown");

        _writer.TryComplete();
        _internalCancellation.Cancel();
    }

    /// <summary>
    ///     Dispose resources
    /// </summary>
    public void Dispose()
    {
        Shutdown();
        _internalCancellation?.Dispose();
    }
}