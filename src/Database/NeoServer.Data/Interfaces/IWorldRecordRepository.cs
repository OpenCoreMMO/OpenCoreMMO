using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IWorldRecordRepository : IBaseRepositoryNeo<WorldRecordEntity>
{
    WorldRecordEntity GetLastFromWord(int worldId);
}