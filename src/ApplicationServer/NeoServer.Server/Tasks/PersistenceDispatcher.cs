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
    private readonly ChannelReader<Action> _reader;
    private readonly ChannelWriter<Action> _writer;
    private volatile bool _isShuttingDown;

    private Task _processingTask;

    /// <summary>
    ///     A queue responsible for processing persistence events
    /// </summary>
    public PersistenceDispatcher(ILogger logger)
    {
        var channel = Channel.CreateUnbounded<Action>(new UnboundedChannelOptions { SingleReader = true });
        _reader = channel.Reader;
        _writer = channel.Writer;
        _logger = logger;
    }

    /// <summary>
    ///     Adds a persistence event to the dispatcher queue
    /// </summary>
    /// <param name="evt"></param>
    public void AddEvent(Action evt)
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

        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _internalCancellation.Token);

        _logger.Debug("PersistenceDispatcher: Starting persistence processing loop");

        _processingTask = Task.Factory.StartNew(() =>
        {
            var eventCount = 0L;

            try
            {
                while (!combinedCts.Token.IsCancellationRequested)
                {
                    if (!_reader.TryRead(out var evt))
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    eventCount++;

                    try
                    {
                        evt();

                        if (eventCount % 100 == 0)
                            _logger.Debug("PersistenceDispatcher: Processed {EventCount} persistence events",
                                eventCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "PersistenceDispatcher: Error during persistence operation");
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
        }, combinedCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        _logger.Debug("PersistenceDispatcher: Started successfully");
    }

    /// <summary>
    ///     Wait for dispatcher completion (useful for tests and shutdown)
    /// </summary>
    public void WaitForCompletion()
    {
        if (_processingTask == null) return;

        try
        {
            _processingTask.Wait();
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