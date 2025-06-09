using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Services;

namespace NeoServer.Server.Events.Creature;

public class CreatureInjuredEventHandler(BloodPoolService bloodPoolService)
    : IApplicationEventHandler<CreatureInjuredEvent>
{
    public void Handle(CreatureInjuredEvent @event)
    {
        var target = @event.Victim;

        foreach (var combatDamage in @event.DamageList)
        {
            if (combatDamage is not { Damage: > 0, IsElementalDamage: false }) continue;
            
            bloodPoolService.CreateSplash(target as ICombatActor, combatDamage);
            return;
        }
    }
}