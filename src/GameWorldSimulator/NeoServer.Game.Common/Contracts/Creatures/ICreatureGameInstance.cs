using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface ICreatureGameInstance
{
    bool TryGetCreature(uint id, out ICreature creature);
    void Add(ICreature creature);
    IEnumerable<ICreature> All();
    bool TryRemove(uint id);
    void AddKilledMonsters(IMonster monster);
    ImmutableList<Tuple<IMonster, TimeSpan>> AllKilledMonsters();
    bool TryRemoveFromKilledMonsters(uint id);
    void AddPlayer(IPlayer player);
    bool TryGetPlayer(uint playerId, out IPlayer player);
    bool TryRemoveFromLoggedPlayers(uint id);
    IEnumerable<IPlayer> AllLoggedPlayers();
    int CountOnlinePlayers();
}