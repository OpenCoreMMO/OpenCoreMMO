using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IIpBansRepository : IBaseRepositoryNeo<IpBanEntity>
{
    Task<IpBanEntity> GetBan(string Ip);
}