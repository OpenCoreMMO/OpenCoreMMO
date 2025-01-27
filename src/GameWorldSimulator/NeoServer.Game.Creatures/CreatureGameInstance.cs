using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures;

public class CreatureGameInstance : ICreatureGameInstance
{
    private readonly Dictionary<uint, ICreature> _creatures;

    private readonly Dictionary<uint, Tuple<IMonster, TimeSpan>> _killedMonsters;
    private readonly Dictionary<uint, IPlayer> _playersLogged;

    public CreatureGameInstance()
    {
        _creatures = new Dictionary<uint, ICreature>();
        _killedMonsters = new Dictionary<uint, Tuple<IMonster, TimeSpan>>();
        _playersLogged = new Dictionary<uint, IPlayer>();

        Instance ??= this;
    }

    internal static CreatureGameInstance Instance { get; private set; }

    public void AddKilledMonsters(IMonster monster)
    {
        if (!monster.BornFromSpawn) return;

        _killedMonsters.TryAdd(monster.CreatureId, new Tuple<IMonster, TimeSpan>(monster, DateTime.Now.TimeOfDay));
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
        return _creatures.Values.ToImmutableArray();
    }

    public IEnumerable<IPlayer> AllLoggedPlayers()
    {
        return _playersLogged.Values;
    }
    
    public int CountOnlinePlayers() => _playersLogged.Count;

    public ImmutableList<Tuple<IMonster, TimeSpan>> AllKilledMonsters()
    {
        return _killedMonsters.Values.ToImmutableList();
    }

    public void Add(ICreature creature)
    {
        if (!_creatures.TryAdd(creature.CreatureId, creature))
            // TODO: proper logging
            Console.WriteLine($"WARNING: Failed to add {creature.Name} to the global dictionary.");
    }

    public void AddPlayer(IPlayer player)
    {
        if (!_playersLogged.TryAdd(player.Id, player))
            // TODO: proper logging
            Console.WriteLine($"WARNING: Failed to add {player.Name} to the global dictionary.");
    }

    public bool TryRemoveFromKilledMonsters(uint id)
    {
        if (!_killedMonsters.Remove(id, out var creature))
        {
            // TODO: proper logging
            Console.WriteLine(
                $"WARNING: Failed to remove {creature.Item1.Name} from the killed monsters dictionary.");
            return false;
        }

        return true;
    }

    public bool TryRemove(uint id)
    {
        if (!_creatures.Remove(id, out var creature))
            // TODO: proper logging
            // Console.WriteLine($"WARNING: Failed to remove {creature.Name} from the global dictionary.");
            return false;
        return true;
    }

    public bool TryRemoveFromLoggedPlayers(uint id)
    {
        if (!_playersLogged.Remove(id, out var player))
            // TODO: proper logging
            // Console.WriteLine($"WARNING: Failed to remove {creature.Name} from the global dictionary.");
            return false;
        return true;
    }
}