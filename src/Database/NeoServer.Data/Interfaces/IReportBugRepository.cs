using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IReportBugRepository : IBaseRepositoryNeo<ReportBugEntity>
{
    ReportBugEntity GetLatestReportPendingByPlayerId(uint playerId);
}