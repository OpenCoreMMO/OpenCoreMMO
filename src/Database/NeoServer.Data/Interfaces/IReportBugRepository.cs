using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IReportBugRepository : IBaseRepositoryNeo<ReportBugEntity>
{
    Task<ReportBugEntity> GetLatestReportPendingByPlayerIdAsync(uint playerId);
}