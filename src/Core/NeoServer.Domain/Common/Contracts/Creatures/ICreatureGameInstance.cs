using System.Collections.Immutable;

namespace NeoServer.Domain.Common.Contracts.Creatures;

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
    List<ICreature> GetCreaturesToCheck(int index);
    void RemoveCreatureFromCheck(int group, int index);
}