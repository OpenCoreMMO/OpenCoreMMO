using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures;

namespace NeoServer.Domain.Services;

/// <summary>
/// Provides functionality to create blood pools or splashes on creature tiles based on their blood type and state.
/// </summary>
public class BloodPoolService(ILiquidPoolFactory liquidPoolFactory)
{
    /// <summary>
    /// Creates a liquid splash at the specified creature's current location, adapting its appearance based on the creature's blood type.
    /// </summary>
    /// <param name="creature">
    /// The creature for which the liquid splash will be created. The creature must implement the <see cref="ICreature"/> interface.
    /// </param>
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

        victim.Tile.ReplaceItemByGroup(pool);
    }

    /// <summary>
    /// Creates a liquid splash at the victim's current location if the creature is not dead, the damage dealt is physical, and greater than zero.
    /// </summary>
    /// <param name="creature">
    /// The creature that received the damage. Must implement the <see cref="ICreature"/> interface.
    /// </param>
    /// <param name="damage">
    /// The damage instance describing the type and magnitude of the damage dealt to the creature.
    /// </param>
    public void CreateSplash(ICreature creature, CombatDamage damage)
    {
        if (creature is not ICombatActor victim) return;
        
        if (damage?.IsElementalDamage ?? false) return;
        if (damage?.Damage <= 0) return;
        if (victim.IsDead) return;

        CreateSplash(victim);
    }

    /// <summary>
    /// Creates a liquid pool at the specified creature's current location based on the creature's blood type.
    /// </summary>
    /// <param name="creature">
    /// The creature for which the liquid pool will be created. The creature must implement the <see cref="ICreature"/> interface.
    /// </param>
    public void CreatePool(ICreature creature)
    {
        if (creature is not ICombatActor victim) return;
        var liquidColor = victim.BloodType switch
        {
            BloodType.Blood => LiquidColor.Red,
            BloodType.Slime => LiquidColor.Green,
            _ => LiquidColor.Red
        };

        var pool = liquidPoolFactory.Create(victim.Location, liquidColor);

        victim.Tile.ReplaceItemByGroup(pool);
    }
}