using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;

namespace NeoServer.Server.Tasks;

public class PersistenceDispatcher : IPersistenceDispatcher
{
    private readonly ILogger _logger;
    private readonly ChannelReader<Func<Task>> _reader;
    private readonly ChannelWriter<Func<Task>> _writer;
    private Task _processingTask;

    /// <summary>
    ///     A queue responsible for process events
    /// </summary>
    public PersistenceDispatcher(ILogger logger)
    {
        var channel = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions { SingleReader = true });
        _reader = channel.Reader;
        _writer = channel.Writer;
        _logger = logger;
    }

    /// <summary>
    ///     Adds an event to dispatcher queue
    /// </summary>
    /// <param name="evt"></param>
    public void AddEvent(Func<Task> evt)
    {
        if (evt is null) return;
        _writer.TryWrite(evt);
    }

    /// <summary>
    ///     Starts dispatcher processing queue
    /// </summary>
    /// <param name="token"></param>
    public void Start(CancellationToken token)
    {
        if (_processingTask != null && !_processingTask.IsCompleted)
        {
            _logger.Warning("PersistenceDispatcher: already started.");
            return;
        }

        _logger.Information("PersistenceDispatcher: starting processing loop.");

        _processingTask = Task.Run(async () =>
        {
            try
            {
                while (await _reader.WaitToReadAsync(token).ConfigureAwait(false))
                {
                    while (_reader.TryRead(out var evt))
                    {
                        try
                        {
                            await evt().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "PersistenceDispatcher: error during persistence operation.");
                        }
                    }
                }

                _logger.Information("PersistenceDispatcher: channel has been completed, stopping processing.");
            }
            catch (OperationCanceledException)
            {
                _logger.Information("PersistenceDispatcher: operation was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PersistenceDispatcher: fatal exception, dispatcher will stop.");
                throw;
            }
            finally
            {
                try
                {
                    _writer.Complete();
                    _logger.Information("PersistenceDispatcher: channel completed.");
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "PersistenceDispatcher: error completing channel.");
                }
            }
        }, CancellationToken.None); // Do not pass token here to avoid automatic task cancellation

        _logger.Information("PersistenceDispatcher: started successfully.");
    }

    // Additional method to wait for dispatcher completion (useful for tests and shutdown)
    public Task WaitForCompletionAsync()
    {
        return _processingTask ?? Task.CompletedTask;
    }
}