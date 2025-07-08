using NeoServer.Domain.Combat.Calculations;
using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Services;
using NeoServer.Domain.World.Algorithms;

namespace NeoServer.Domain.Combat.Attacks;

public class AreaAttackService(
    IEventAggregator eventAggregator,
    IMap map,
    MagicFieldService magicFieldService,
    ConditionAttackService conditionAttackService) : IAttackService
{
    public CombatResult Execute(AttackInput attackInput)
    {
        var damage = DamageCalculation.Calculate(attackInput);

        var totalDamage = (uint) PerformAreaAttack(attackInput, damage);

        return new CombatResult(totalDamage, Result.Success);
    }

    private int PerformAreaAttack(AttackInput attackInput, CalculatedAttackDamage damage)
    {
        if (!attackInput.Parameters.IsAttackInArea) return 0;

        var aggressor = attackInput.Aggressor as ICombatActor;

        var targetlocation = attackInput.Target.Location;

        var area = attackInput.Parameters.CoordinateArea ??
                       AreaEffect.Create(targetlocation, attackInput.Parameters.Area);

        var affectedArea = new List<Location>(area.Length);
        var affectedCreatures = new List<ICreature>();

        foreach (var coordinate in area)
        {
            var location = coordinate.Location;
            var tile = map[location];

            // Check if the tile is walkable and clear of obstacles
            if (tile is not IDynamicTile walkableTile || walkableTile.HasFlag(TileFlags.Unpassable) ||
                walkableTile.ProtectionZone || walkableTile.HasHole)
                continue;

            // Check if the line of sight is clear between aggressor and target location
            if (!SightClear.IsSightClear(map, attackInput.Aggressor.Location, tile.Location, false)) continue;

            affectedArea.Add(location);

            if (attackInput.Parameters.FieldAttack) CreateMagicField(attackInput, tile);

            var targetCreatures = walkableTile.Creatures?.ToArray();
            if (targetCreatures is null or { Length: 0 }) continue;

            affectedCreatures.AddRange(targetCreatures);
        }

        eventAggregator.Publish(new CreatureAttackingEvent(aggressor, attackInput.Target,
            attackInput.Parameters.ShootType,
            attackInput.Parameters.Effect, false, affectedArea.ToArray()));

        var totalDamage = 0;

        foreach (var affectedCreature in affectedCreatures)
        {
            if (affectedCreature is not ICombatActor target) continue;
            if (affectedCreature.Equals(aggressor)) continue;

            var unjustifiedAttack =
                target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
                playerAggressor.GetSkull(targetPlayer) is Skull.None;

            var mainDamage = damage.MainDamage;

            if (mainDamage is { Damage: > 0, Type: not DamageType.None })
            {
                mainDamage.Unjustified = unjustifiedAttack;

                var damageResult = InflictDamage(damage, mainDamage, target, aggressor);

                totalDamage += damageResult.DamageList.TotalDamage;
                
                if (damageResult.WasDamaged) conditionAttackService.Execute(attackInput);
            }
            else
            {
                conditionAttackService.Execute(attackInput);
            }
        }

        return totalDamage;
    }

    private void CreateMagicField(AttackInput attackInput, ITile tile)
    {
        var magicFieldType = attackInput.Parameters.DamageType switch
        {
            DamageType.Earth => MagicFieldType.Poison,
            DamageType.Energy => MagicFieldType.Energy,
            DamageType.Fire => MagicFieldType.Fire,
            _ => MagicFieldType.None
        };

        magicFieldService.AddToGround(attackInput.Aggressor as ICreature, tile, magicFieldType);
    }

    private static DamageResult InflictDamage(CalculatedAttackDamage damage, CombatDamage mainDamage, ICombatActor target,
        IThing aggressor)
    {
        if (damage.ExtraDamage is { Damage: > 0, Type: not DamageType.None })
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            return target.TakeDamage(aggressor, damages);
        }

        return target.TakeDamage(aggressor, damage.MainDamage);
    }
}

public static class AreaRotationHelper
{
    public static byte[,] Rotate(byte[,] matrix, Direction direction)
    {
        return direction switch
        {
            Direction.West => matrix,
            Direction.East => Rotate180(matrix),
            Direction.North => Rotate270(matrix),
            Direction.South => Rotate90(matrix),
            Direction.NorthWest => matrix,
            Direction.NorthEast => Mirror(matrix),
            Direction.SouthWest => Flip(matrix),
            Direction.SouthEast => Mirror(Flip(matrix)),
            _ => matrix
        };
    }

    private static byte[,] Mirror(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                result[y, x] = matrix[y, cols - 1 - x];

        return result;
    }

    private static byte[,] Flip(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                result[y, x] = matrix[rows - 1 - y, x];

        return result;
    }


    public static byte[,] Rotate90(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new byte[cols, rows];

        for (int i = 0; i < rows; ++i)
            for (int j = 0; j < cols; ++j)
                result[j, rows - i - 1] = matrix[i, j];

        return result;
    }

    public static byte[,] Rotate180(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new byte[rows, cols];

        for (int i = 0; i < rows; ++i)
            for (int j = 0; j < cols; ++j)
                result[rows - i - 1, cols - j - 1] = matrix[i, j];

        return result;
    }

    public static byte[,] Rotate270(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var result = new byte[cols, rows];

        for (int i = 0; i < rows; ++i)
            for (int j = 0; j < cols; ++j)
                result[cols - j - 1, i] = matrix[i, j];

        return result;
    }

    // Rotação diagonal simplificada: espelha a matriz
    public static byte[,] Rotate45(byte[,] matrix, Direction diagonal)
    {
        var rot = diagonal switch
        {
            Direction.NorthEast => Rotate90(matrix),
            Direction.SouthEast => Rotate180(Rotate90(matrix)),
            Direction.SouthWest => Rotate180(Rotate270(matrix)),
            Direction.NorthWest => Rotate270(matrix),
            _ => matrix
        };

        return rot;
    }

    public static bool IsCircularArea(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        // Deve ser quadrada e com dimensões ímpares
        if (rows != cols || rows % 2 == 0)
            return false;

        int center = rows / 2;

        if (matrix[center, center] != 3)
            return false;

        // Verifica simetria vertical e horizontal
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (matrix[y, x] != matrix[rows - 1 - y, x])
                    return false;

                if (matrix[y, x] != matrix[y, cols - 1 - x])
                    return false;
            }
        }

        return true;
    }

}
