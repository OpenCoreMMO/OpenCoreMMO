using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Depot;
using NeoServer.Domain.Items.Items.Containers;

namespace NeoServer.Data.Interfaces;

public interface IPlayerDepotItemRepository : IBaseRepositoryNeo<PlayerDepotItemEntity>
{
    Task<IEnumerable<PlayerDepotItemEntity>> GetByPlayerId(uint id);
    Task Save(IPlayer player, Depot depot);
}