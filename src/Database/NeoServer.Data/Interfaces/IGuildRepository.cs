using System.Collections.Generic;

using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IGuildRepository : IBaseRepositoryNeo<GuildEntity>
{
    new IEnumerable<GuildEntity> GetAll();
    GuildEntity GetByName(string name);
    GuildEntity GetById(int id);
}