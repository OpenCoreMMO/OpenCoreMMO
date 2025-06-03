using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Combat.Services;

public class CombatBloodPoolService(IMap map, ILiquidPoolFactory liquidPoolFactory)
{
    public void CreateSplash(ICreature creature)
    {
        if (creature is not ICombatActor victim) return;

        var liquidColor = victim.BloodType switch
        {
            BloodType.Blood => LiquidColor.Red,
            BloodType.Slime => LiquidColor.Green,

            _ => LiquidColor.Red
        };

        var pool = liquidPoolFactory.CreateDamageLiquidPool(victim.Location, liquidColor);

        map.CreateBloodPool(pool, victim.Tile);
    }

    public void CreateSplash(ICreature creature, CombatDamage damage)
    {
        if (creature is not ICombatActor victim) return;

        if (damage?.IsElementalDamage ?? false) return;
        if (damage?.Damage <= 0) return;
        if (victim.IsDead) return;

        CreateSplash(victim);
    }
}