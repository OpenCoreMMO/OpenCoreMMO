using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Server.Managers;

/// <summary>
///     Control creatures on game
/// </summary>
public class GameCreatureManager(
    ICreatureGameInstance creatureInstances,
    IMap map,
    ILogger logger,
    IWorldRecordRepository worldRecordRepository)
    : IGameCreatureManager
{
    private readonly ConcurrentDictionary<uint, IConnection> _playersConnection = new();

    /// <summary>
    ///     Gets all creatures in game
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ICreature> GetCreatures()
    {
        return creatureInstances.All();
    }

    /// <summary>
    ///     Adds killed monsters
    /// </summary>
    /// <param name="monster"></param>
    public void AddKilledMonsters(IMonster monster)
    {
        creatureInstances.TryRemove(monster.CreatureId);
        creatureInstances.AddKilledMonsters(monster);
    }

    /// <summary>
    ///     Gets creature instance on game
    /// </summary>
    /// <param name="id">Creature Id</param>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool TryGetPlayer(uint id, out IPlayer player)
    {
        player = default;
        if (TryGetCreature(id, out var creature) && creature is IPlayer)
        {
            player = (IPlayer)creature;
            return true;
        }

        return false;
    }

    public virtual bool TryGetPlayer(string name, out IPlayer player)
    {
        player = default;

        if (string.IsNullOrWhiteSpace(name)) return false;

        var creature = creatureInstances.All().FirstOrDefault(x =>
            x is IPlayer playerFound &&
            playerFound.Name.Trim().Equals(name.Trim(), StringComparison.InvariantCultureIgnoreCase));

        if (creature is not IPlayer p) return false;

        player = p;
        return true;
    }

    public bool IsPlayerLogged(IPlayer player)
    {
        return creatureInstances.TryGetPlayer(player.Id, out player);
    }

    public bool TryGetLoggedPlayer(uint playerId, out IPlayer player)
    {
        return creatureInstances.TryGetPlayer(playerId, out player);
    }

    public IEnumerable<IPlayer> GetAllLoggedPlayers()
    {
        return creatureInstances.AllLoggedPlayers();
    }

    /// <summary>
    ///     Returns a creature by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="creature"></param>
    /// <returns></returns>
    public bool TryGetCreature(uint id, out ICreature creature)
    {
        return creatureInstances.TryGetCreature(id, out creature);
    }

    /// <summary>
    ///     Removes creature from game
    ///     This method also removes creature from map
    /// </summary>
    /// <param name="creature"></param>
    /// <returns></returns>
    public bool RemoveCreature(ICreature creature)
    {
        if (creature is IWalkableCreature walkableCreature) map.RemoveCreature(walkableCreature);

        creatureInstances.TryRemove(creature.CreatureId);

        return true;
    }

    /// <summary>
    ///     Adds player to game
    ///     This method also adds player to map and to connection pool
    /// </summary>
    /// <param name="player"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
    public IPlayer AddPlayer(IPlayer player, IConnection connection)
    {
        var playerIsLogged = creatureInstances.TryGetPlayer(player.Id, out var playerLogged);
        player = playerLogged ?? player;

        connection.SetConnectionOwner(player);

        _playersConnection.AddOrUpdate(player.CreatureId, connection, (_, _) => connection);

        if (playerIsLogged) return player;

        AddCreature(player);
        creatureInstances.AddPlayer(player);

        return player;
    }

    /// <summary>
    ///     Removes player from game, map and connection pool
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool RemovePlayer(IPlayer player)
    {
        if (_playersConnection.TryRemove(player.CreatureId, out var connection))
            connection.Disconnect();

        creatureInstances.TryRemoveFromLoggedPlayers(player.Id);

        RemoveCreature(player);

        logger.Information("{Player} was removed from game", player.Name);

        return true;
    }

    /// <summary>
    ///     Gets the player connection
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
    public virtual bool GetPlayerConnection(uint playerId, out IConnection connection)
    {
        return _playersConnection.TryGetValue(playerId, out connection);
    }

    public async Task<(bool, int, int)> CheckPlayersRecord(int worldId)
    {
        var actualCount = creatureInstances.AllLoggedPlayers().Count();
        var lastWorldRecord = await worldRecordRepository.GetLastFromWord(worldId);

        if (lastWorldRecord is null || actualCount > lastWorldRecord.Record)
        {
            await worldRecordRepository.Insert(new WorldRecordEntity
            {
                CreatedAt = DateTime.UtcNow,
                Record = actualCount,
                WorldId = worldId
            });

            return (true, actualCount, lastWorldRecord is null ? 0 : lastWorldRecord.Record);
        }

        return (false, 0, 0);
    }

    public List<ICreature> GetCreaturesToCheck(int index)
    {
        return creatureInstances.GetCreaturesToCheck(index);
    }

    public void RemoveCreatureFromCheck(int group, int index)
    {
        creatureInstances.RemoveCreatureFromCheck(group, index);
    }

    /// <summary>
    ///     Adds creature to game and to map
    /// </summary>
    /// <param name="creature"></param>
    /// <returns></returns>
    private bool AddCreature(ICreature creature)
    {
        creatureInstances.Add(creature);
        return true;
    }

    /// <summary>
    ///     Returns all killed monsters
    /// </summary>
    /// <returns></returns>
    public IImmutableList<Tuple<IMonster, TimeSpan>> GetKilledMonsters()
    {
        return creatureInstances.AllKilledMonsters();
    }
}