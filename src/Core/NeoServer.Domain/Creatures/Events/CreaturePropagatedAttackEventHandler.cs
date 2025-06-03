using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Creatures.Events;

public class CreaturePropagatedAttackEventHandler : IGameEventHandler
{
    private readonly IMap map;

    public CreaturePropagatedAttackEventHandler(IMap map)
    {
        this.map = map;
    }

    public void Execute(ICombatActor actor, CombatDamage damage, AffectedLocation[] area)
    {
        map.PropagateAttack(actor, damage, area);
    }
}