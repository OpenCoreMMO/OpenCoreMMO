using System.Collections.Generic;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Interfaces;

public interface IPlayerDeathRepository: IBaseRepositoryNeo<PlayerDeathEntity>
{
    IEnumerable<PlayerDeathEntity> GetPlayerKills(int playerId);
}