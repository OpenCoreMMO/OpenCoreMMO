using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IIpBansRepository : IBaseRepositoryNeo<IpBanEntity>
{
    IpBanEntity ExistBan(string Ip);
}