using System.Collections.Generic;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.Services;

public interface ICreatureDeathService
{
    void Handle(ICombatActor deadCreature, IThing by, List<DamageRecord> damageRecords);
}