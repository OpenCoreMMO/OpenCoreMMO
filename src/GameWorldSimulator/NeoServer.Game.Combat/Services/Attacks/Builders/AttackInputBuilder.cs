using NeoServer.Game.Combat.Services.Attacks.Builders.AttackParameter;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Combat.Services.Attacks.Builders;

public static class AttackInputBuilder
{
    public static AttackInput Build(IThing aggressor, IThing target)
    {
        var attackParameter = new Common.Combat.Structs.AttackParameter
        {
            Type = AttackType.None
        };
        
        if (aggressor is IPlayer player)
        {
            attackParameter = PlayerAttackParameterBuilder.Build(player, target);    
        }
        
        return new AttackInput(aggressor, target)
        {
            Parameters = attackParameter
        };
    }
}