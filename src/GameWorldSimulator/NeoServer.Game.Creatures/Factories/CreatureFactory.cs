using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Creatures.Factories;

public class CreatureFactory : ICreatureFactory
{
    private readonly IEnumerable<ICreatureEventSubscriber> _creatureEventSubscribers;

    //factories
    private readonly IMonsterFactory _monsterFactory;
    private readonly INpcFactory _npcFactory;

    public CreatureFactory(
        IMonsterFactory monsterFactory,
        IEnumerable<ICreatureEventSubscriber> creatureEventSubscribers, INpcFactory npcFactory)
    {
        _monsterFactory = monsterFactory;
        _creatureEventSubscribers = creatureEventSubscribers;
        Instance = this;
        _npcFactory = npcFactory;
    }

    public static ICreatureFactory Instance { get; private set; }
    public event CreatureCreated OnCreatureCreated;

    public IMonster CreateMonster(string name, ISpawnPoint spawn = null)
    {
        var monster = _monsterFactory.Create(name, spawn);
        if (monster is null) return null;

        AttachEvents(monster);

        OnCreatureCreated?.Invoke(monster);
        return monster;
    }

    public IMonster CreateSummon(string name, ICreature master)
    {
        var monster = _monsterFactory.CreateSummon(name, master);
        if (monster is null) return null;

        AttachEvents(monster);

        OnCreatureCreated?.Invoke(monster);
        return monster;
    }

    public INpc CreateNpc(string name, ISpawnPoint spawn = null)
    {
        var npc = _npcFactory.Create(name, spawn);
        if (npc is null) return null;

        AttachEvents(npc);
        OnCreatureCreated?.Invoke(npc);

        return npc;
    }

    public IPlayer CreatePlayer(IPlayer player)
    {
        var createdPlayer = AttachEvents(player) as IPlayer;

        OnCreatureCreated?.Invoke(player);
        return createdPlayer;
    }

    private ICreature AttachEvents(ICreature creature)
    {
        var gameEventSubscriberTypes =
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsAssignableTo(typeof(IGameEventSubscriber)))
                .Select(x => x.FullName)
                .ToHashSet();

        _creatureEventSubscribers.AsParallel().ForAll(subscriber =>
        {
            if (!gameEventSubscriberTypes.Contains(subscriber.GetType().FullName)) return;
            if (!subscriber.GetType().IsAssignableTo(typeof(IGameEventSubscriber))) return;

            subscriber?.Subscribe(creature);
        });

        return creature;
    }
}