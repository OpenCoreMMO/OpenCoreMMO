using System.Collections.Immutable;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Models.Bases;
using Serilog;

namespace NeoServer.Domain.Creatures;

public class CreatureGameInstance : ICreatureGameInstance
{
    private readonly ILogger _logger;
    public const ushort CREATURE_COUNT = 10;
    
    private readonly Dictionary<uint, ICreature> _creatures;
    private readonly Dictionary<uint, Tuple<IMonster, TimeSpan>> _killedMonsters;
    private readonly Dictionary<uint, IPlayer> _playersLogged;

    private readonly List<ICreature>[] _creaturesCheck;
    private readonly Random _creatureGroupRandom = new();

    public CreatureGameInstance(ILogger logger)
    {
        _logger = logger;
        _creatures = new Dictionary<uint, ICreature>();
        _killedMonsters = new Dictionary<uint, Tuple<IMonster, TimeSpan>>();
        _playersLogged = new Dictionary<uint, IPlayer>();

        _creaturesCheck = new List<ICreature>[CREATURE_COUNT];
        Instance ??= this;
    }

    internal static CreatureGameInstance Instance { get; private set; }

    public List<ICreature> GetCreaturesToCheck(int index) => _creaturesCheck[index];

    public void RemoveCreatureFromCheck(int group, int index)
    {
        _creaturesCheck[group][index] = _creaturesCheck[group][^1];
        _creaturesCheck[group].RemoveAt(_creaturesCheck[group].Count - 1);
    }

    public void AddKilledMonsters(IMonster monster)
    {
        if (!monster.BornFromSpawn) return;

        _killedMonsters.TryAdd(monster.CreatureId, new Tuple<IMonster, TimeSpan>(monster, DateTime.UtcNow.TimeOfDay));
    }

    public bool TryGetCreature(uint id, out ICreature creature)
    {
        return _creatures.TryGetValue(id, out creature);
    }

    public bool TryGetPlayer(uint playerId, out IPlayer player)
    {
        return _playersLogged.TryGetValue(playerId, out player);
    }

    public IEnumerable<ICreature> All()
    {
        return [.._creatures.Values];
    }

    public IEnumerable<IPlayer> AllLoggedPlayers()
    {
        return _playersLogged.Values;
    }

    public int CountOnlinePlayers()
    {
        return _playersLogged.Count;
    }

    public ImmutableList<Tuple<IMonster, TimeSpan>> AllKilledMonsters()
    {
        return _killedMonsters.Values.ToImmutableList();
    }

    public void Add(ICreature creature)
    {
        if (!_creatures.TryAdd(creature.CreatureId, creature))
        {
            _logger.Warning("Failed to add {CreatureName} to the global dictionary", creature.Name);
            return;
        }

        var index = _creatureGroupRandom.Next(CREATURE_COUNT);
        _creaturesCheck[index] ??= [];
        
        _creaturesCheck[index].Add(creature);
    }

    public void AddPlayer(IPlayer player)
    {
        if (!_playersLogged.TryAdd(player.Id, player))
        {
            _logger.Warning("Failed to add {PlayerName} to the global dictionary", player.Name);
        }
    }

    public bool TryRemoveFromKilledMonsters(uint id)
    {
        if (!_killedMonsters.Remove(id, out var creature))
        {
            _logger.Warning("Failed to remove creature with id {Id} from the killed monsters dictionary", id);
            return false;
        }

        return true;
    }

    public bool TryRemove(uint id)
    {
        if (!_creatures.Remove(id, out _))
        {
            _logger.Warning("Failed to remove creature with id {Id} from the global dictionary", id);

            return false;
        }

        return true;
    }

    public bool TryRemoveFromLoggedPlayers(uint id)
    {
        if (!_playersLogged.Remove(id, out _))
        {
            _logger.Warning("Failed to remove player with id {PlayerId} from the global dictionary", id);
            return false;
        }

        return true;
    }
}