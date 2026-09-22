namespace NeoServer.Domain.Repositories;

public interface IPlayerRepository
{
    Task<int> GetIdByName(string name);
    Task UpdateBankAmount(int playerId, ulong amount);
}