using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ICreatureDeathService
{
    void Handle(ICombatActor deadCreature, IThing by, List<DamageRecord> damageRecords);
}