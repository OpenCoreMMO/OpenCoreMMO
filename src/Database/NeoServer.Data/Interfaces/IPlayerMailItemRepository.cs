using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Locker;

namespace NeoServer.Data.Interfaces;

public interface IPlayerMailItemRepository : IBaseRepositoryNeo<PlayerMailItemEntity>
{
    Task<IEnumerable<PlayerMailItemEntity>> GetByPlayerId(uint id);
    Task Save(IPlayer player, IContainer locker);
}