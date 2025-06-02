using System.Collections.Generic;
using NeoServer.Data.Entities;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Data.Interfaces;

public interface IPlayerDeathRepository : IBaseRepositoryNeo<PlayerDeathEntity>
{
    IEnumerable<PlayerDeathEntity> GetPlayerKills(int playerId);
    new void Save(IPlayer deadPlayer, DamageRecordResult damageRecordResult);
}