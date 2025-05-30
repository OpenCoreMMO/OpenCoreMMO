using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IGuildRepository : IBaseRepositoryNeo<GuildEntity>
{
    Task<IEnumerable<GuildEntity>> GetAll();
    Task<GuildEntity> GetByName(string name);
    Task<GuildEntity> GetById(int id);
}