using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IWorldRecordRepository : IBaseRepositoryNeo<WorldRecordEntity>
{
    Task<WorldRecordEntity> GetLastFromWord(int worldId);
}