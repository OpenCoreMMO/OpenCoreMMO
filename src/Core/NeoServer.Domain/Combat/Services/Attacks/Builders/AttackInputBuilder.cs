using NeoServer.Domain.Combat.Services.Attacks.Builders.AttackParameter;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Combat.Services.Attacks.Builders;

public static class AttackInputBuilder
{
    public static AttackInput Build(IThing aggressor, IThing target)
    {
        var combatParameter = new CombatParameter();

        if (aggressor is IPlayer player) combatParameter = PlayerCombatParameterBuilder.Build(player, target);

        return new AttackInput(aggressor, target, combatParameter);
    }
}