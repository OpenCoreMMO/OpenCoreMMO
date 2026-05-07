using System.Collections.Generic;

using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Data.Interfaces;

public interface IPlayerDepotItemRepository : IBaseRepositoryNeo<PlayerDepotItemEntity>
{
    IEnumerable<PlayerDepotItemEntity> GetByPlayerId(uint id);
    void Save(IPlayer player, IContainer depotChest);
}