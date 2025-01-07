using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT;

public class AttachEventLoaders : IRunBeforeLoaders
{
    private readonly IEnumerable<ICreatureEventSubscriber> _creatureEventSubscribers;
    private readonly ICreatureFactory _creatureFactory;

    public AttachEventLoaders(ICreatureFactory creatureFactory,
        IEnumerable<ICreatureEventSubscriber> creatureEventSubscribers)
    {
        _creatureFactory = creatureFactory;
        _creatureEventSubscribers = creatureEventSubscribers;
    }

    public void Run()
    {
        _creatureFactory.OnCreatureCreated += OnCreatureCreated;
    }

    private void OnCreatureCreated(ICreature creature)
    {
        AttachEvents(creature);
    }

    private void AttachEvents(ICreature creature)
    {
        var gameEventSubscriberTypes =
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ICreatureEventSubscriber)))
                .Select(x => x.FullName)
                .ToHashSet();

        _creatureEventSubscribers.AsParallel().ForAll(subscriber =>
        {
            if (!gameEventSubscriberTypes.Contains(subscriber.GetType().FullName)) return;

            subscriber?.Subscribe(creature);
        });
    }
}