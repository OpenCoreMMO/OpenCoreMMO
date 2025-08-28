using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Repositories;

public interface IPlayerRepository
{
    Task<int> GetIdByName(string name);
}