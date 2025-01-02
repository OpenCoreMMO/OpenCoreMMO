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
        Task.Factory.StartNew(async () =>
        {
            while (await _reader.WaitToReadAsync(token))
            {
                if (token.IsCancellationRequested) _writer.Complete();
                // Fast loop around available jobs
                while (_reader.TryRead(out var evt))

                    try
                    {
                        await evt.Invoke(); //execute event
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error found during persistence operation");
                    }
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
}