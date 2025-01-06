using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IAccountRepository : IBaseRepositoryNeo<AccountEntity>
{
    Task<AccountEntity> GetAccount(string name, string password);
    Task AddPlayerToVipList(int accountId, int playerId);
    Task RemoveFromVipList(int accountId, int playerId);
    Task<PlayerEntity> GetPlayer(string accountName, string password, string charName);
    Task<PlayerEntity> GetOnlinePlayer(string accountName);
    Task<int> Ban(uint accountId, string reason, uint bannedByAccountId);
    Task<AccountEntity> GetByEmailOrAccountName(string email, string accountName);
}