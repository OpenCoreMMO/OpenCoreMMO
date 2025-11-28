using System.Collections.Immutable;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures;

public class CreatureGameInstance : ICreatureGameInstance
{
    private readonly List<ICreature> _creaturesArray;
    private readonly Dictionary<uint, int> _creatures;

    private readonly List<Tuple<IMonster, TimeSpan>> _killedMonstersArray;
    private readonly Dictionary<uint, int> _killedMonsters;
    
    private readonly List<IPlayer> _playersLoggedArray;
    private readonly Dictionary<uint, int> _playersLogged;

    public CreatureGameInstance()
    {
        _creaturesArray = new List<ICreature>();
        _creatures = new Dictionary<uint, int>();
        
        _killedMonstersArray = new List<Tuple<IMonster, TimeSpan>>();
        _killedMonsters = new Dictionary<uint, int>();
        
        _playersLoggedArray = new List<IPlayer>();
        _playersLogged = new Dictionary<uint, int>();

        Instance ??= this;
    }

    internal static CreatureGameInstance Instance { get; private set; }

    public void AddKilledMonsters(IMonster monster)
    {
        if (!monster.BornFromSpawn) return;

        if (!_killedMonsters.ContainsKey(monster.CreatureId))
        {
            var tuple = new Tuple<IMonster, TimeSpan>(monster, DateTime.UtcNow.TimeOfDay);
            _killedMonstersArray.Add(tuple);
            _killedMonsters.TryAdd(monster.CreatureId, _killedMonstersArray.Count - 1);
        }
    }

    public bool TryGetCreature(uint id, out ICreature creature)
    {
        creature = null;
        if (_creatures.TryGetValue(id, out var index) && index < _creaturesArray.Count)
        {
            creature = _creaturesArray[index];
            return creature != null;
        }
        return false;
    }

    public bool TryGetPlayer(uint playerId, out IPlayer player)
    {
        player = null;
        if (_playersLogged.TryGetValue(playerId, out var index) && index < _playersLoggedArray.Count)
        {
            player = _playersLoggedArray[index];
            return player != null;
        }
        return false;
    }

    public IEnumerable<ICreature> All()
    {
        return _creaturesArray;
    }

    public IEnumerable<IPlayer> AllLoggedPlayers()
    {
        return _playersLoggedArray;
    }

    public int CountOnlinePlayers()
    {
        return _playersLogged.Count;
    }

    public ImmutableList<Tuple<IMonster, TimeSpan>> AllKilledMonsters()
    {
        return _killedMonstersArray.ToImmutableList();
    }

    public void Add(ICreature creature)
    {
        if (!_creatures.ContainsKey(creature.CreatureId))
        {
            _creaturesArray.Add(creature);
            if (!_creatures.TryAdd(creature.CreatureId, _creaturesArray.Count - 1))
                // TODO: proper logging
                Console.WriteLine($"WARNING: Failed to add {creature.Name} to the global dictionary.");
        }
        else
        {
            // TODO: proper logging
            Console.WriteLine($"WARNING: Failed to add {creature.Name} to the global dictionary.");
        }
    }

    public void AddPlayer(IPlayer player)
    {
        if (!_playersLogged.ContainsKey(player.Id))
        {
            _playersLoggedArray.Add(player);
            if (!_playersLogged.TryAdd(player.Id, _playersLoggedArray.Count - 1))
                // TODO: proper logging
                Console.WriteLine($"WARNING: Failed to add {player.Name} to the global dictionary.");
        }
        else
        {
            // TODO: proper logging
            Console.WriteLine($"WARNING: Failed to add {player.Name} to the global dictionary.");
        }
    }

    public bool TryRemoveFromKilledMonsters(uint id)
    {
        if (_killedMonsters.TryGetValue(id, out var index))
        {
            var lastIndex = _killedMonstersArray.Count - 1;
            
            if (index < lastIndex)
            {
                // Swap with last element
                var lastItem = _killedMonstersArray[lastIndex];
                _killedMonstersArray[index] = lastItem;
                
                // Update the dictionary for the swapped item
                _killedMonsters[lastItem.Item1.CreatureId] = index;
            }
            
            // Remove last element
            _killedMonstersArray.RemoveAt(lastIndex);
            _killedMonsters.Remove(id);
            return true;
        }
        
        return false;
    }

    public bool TryRemove(uint id)
    {
        if (_creatures.TryGetValue(id, out var index))
        {
            var lastIndex = _creaturesArray.Count - 1;
            
            if (index < lastIndex)
            {
                // Swap with last element
                var lastItem = _creaturesArray[lastIndex];
                _creaturesArray[index] = lastItem;
                
                // Update the dictionary for the swapped item
                _creatures[lastItem.CreatureId] = index;
            }
            
            // Remove last element
            _creaturesArray.RemoveAt(lastIndex);
            _creatures.Remove(id);
            return true;
        }
        return false;
    }

    public bool TryRemoveFromLoggedPlayers(uint id)
    {
        if (_playersLogged.TryGetValue(id, out var index))
        {
            var lastIndex = _playersLoggedArray.Count - 1;
            
            if (index < lastIndex)
            {
                // Swap with last element
                var lastItem = _playersLoggedArray[lastIndex];
                _playersLoggedArray[index] = lastItem;
                
                // Update the dictionary for the swapped item
                _playersLogged[lastItem.Id] = index;
            }
            
            // Remove last element
            _playersLoggedArray.RemoveAt(lastIndex);
            _playersLogged.Remove(id);
            return true;
        }
        return false;
    }
}