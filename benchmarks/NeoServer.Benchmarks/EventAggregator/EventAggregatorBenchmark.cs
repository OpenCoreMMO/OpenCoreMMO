using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Benchmarks.EventAggregator;

[SimpleJob(RunStrategy.ColdStart, 100)]
[MemoryDiagnoser]
public class EventAggregatorBenchmark
{
    private readonly EventAggregatorUsingObjectPool _eventAggregatorUsingObjectPool;
    private readonly EventAggregatorWithoutObjectPool _eventAggregatorWithoutObjectPool;

    public EventAggregatorBenchmark()
    {
        var builder = new ServiceCollection();
        builder.AddSingleton<IBaseEventHandler<PlayerChangedEvent>, EventHandler>();
        var serviceProvider = builder.BuildServiceProvider();
        
        _eventAggregatorUsingObjectPool = new EventAggregatorUsingObjectPool();
        //_eventAggregatorUsingObjectPool.Initialize(serviceProvider);
        
        _eventAggregatorWithoutObjectPool = new EventAggregatorWithoutObjectPool(serviceProvider);
        //_eventAggregatorWithoutObjectPool.Initialize();
    }

    [Benchmark]
    public void PublishToEventAggregatorUsingObjectPool()
    {
        for (int i = 0; i < 10_000; i++)
        {
            _eventAggregatorUsingObjectPool.Publish<PlayerChangedEvent>((e) =>
            {
                e.Level = 100;
                e.Name = "Test";
            });
        }
    }
    
    [Benchmark]
    public void PublishToEventAggregatorWithoutObjectPool()
    {
        for (int i = 0; i < 10_000; i++)
        {
            _eventAggregatorWithoutObjectPool.Publish(new PlayerChangedEvent
            {
                Level = 100,
                Name = "Test"
            });
        }
    }
    [Benchmark]
    public void PublishToEventAggregatorWithoutObjectPoolStaticEvent()
    {
        var @event = new PlayerChangedEvent
        {
            Level = 100,
            Name = "Test"
        };
            
        for (int i = 0; i < 10_000; i++)
        {
            @event.Level = 100;
            @event.Name = "Test";
            _eventAggregatorWithoutObjectPool.Publish(@event);
        }
    }
}

public class PlayerChangedEvent : IBaseEvent, IIntegrationEvent
{
    public string Name { get; set; }
    public int Level { get; set; }
    public void Reset()
    {
    }
}
public class EventHandler: IBaseEventHandler<PlayerChangedEvent>
{
    public void Handle(PlayerChangedEvent @event)
    {
    }
}