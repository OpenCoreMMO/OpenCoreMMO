using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;

namespace NeoServer.Server.Tasks;

public class Dispatcher : IDispatcher
{
    private readonly ILogger _logger;
    private readonly ChannelReader<IEvent> _reader;
    private readonly ChannelWriter<IEvent> _writer;

    /// <summary>
    ///     A queue responsible for process events
    /// </summary>
    public Dispatcher(ILogger logger)
    {
        var channel = Channel.CreateUnbounded<IEvent>(new UnboundedChannelOptions { SingleReader = true });
        _reader = channel.Reader;
        _writer = channel.Writer;
        _logger = logger;
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
                if (token.IsCancellationRequested) _writer.Complete();
                // Fast loop around available jobs
                while (_reader.TryRead(out var evt))
                {
                    if (!evt.HasExpired || evt.HasNoTimeout)
                        try
                        {
                            evt.Action?.Invoke(); //execute event
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