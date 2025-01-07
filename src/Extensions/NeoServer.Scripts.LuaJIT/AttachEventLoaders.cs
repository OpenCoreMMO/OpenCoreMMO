using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT;

public class AttachEventLoaders(
    ICreatureFactory creatureFactory,
    IEnumerable<ICreatureEventSubscriber> creatureEventSubscribers)
    : IRunBeforeLoaders
{
    public void Run()
    {
        creatureFactory.OnCreatureCreated += OnCreatureCreated;
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

        creatureEventSubscribers.AsParallel().ForAll(subscriber =>
        {
            if (!gameEventSubscriberTypes.Contains(subscriber.GetType().FullName)) return;

            subscriber?.Subscribe(creature);
        });
    }
}