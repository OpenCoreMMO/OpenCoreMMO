using System.Collections.Generic;

using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IAccountRepository : IBaseRepositoryNeo<AccountEntity>
{
    AccountEntity GetAccount(string name, string password);
    void AddPlayerToVipList(int accountId, int playerId);
    void RemoveFromVipList(int accountId, int playerId);

    PlayerEntity GetPlayer(string accountName, string password, string charName, bool includeDeathList = false,
        bool includeKillsLastMonth = false);

    IList<PlayerEntity> GetOnlinePlayers(string accountName);
    int Ban(uint accountId, string reason, uint bannedByAccountId);
}