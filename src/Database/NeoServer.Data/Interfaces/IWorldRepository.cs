using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IWorldRepository : IBaseRepositoryNeo<WorldEntity>
{
    Task<IEnumerable<WorldEntity>> GetPaginatedWorldsAsync(Expression<Func<WorldEntity, bool>> filter, int page, int limit);
    Task<WorldEntity> GetByNameOrIpPort(string name, string ip, int port);
}