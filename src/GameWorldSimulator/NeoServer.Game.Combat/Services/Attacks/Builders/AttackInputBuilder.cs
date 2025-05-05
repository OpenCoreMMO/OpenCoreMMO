using NeoServer.Game.Combat.Services.Attacks.Builders.AttackParameter;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Combat.Services.Attacks.Builders;

public static class AttackInputBuilder
{
    public static AttackInput Build(IThing aggressor, IThing target)
    {
        var attackParameter = new Services.AttackParameter
        {
            Type = AttackType.None
        };
        
        if (aggressor is IPlayer player)
        {
            attackParameter = PlayerAttackParameterBuilder.Build(player);    
        }
        
        return new AttackInput(aggressor, target)
        {
            Parameters = attackParameter
        };
    }
}