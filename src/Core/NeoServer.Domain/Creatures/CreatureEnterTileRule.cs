using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster.Summon;

namespace NeoServer.Domain.Creatures;

public abstract class CreatureEnterTileRule<T> : ITileEnterRule
{
    private static readonly Lazy<T> Lazy = new(() => (T)Activator.CreateInstance(typeof(T), true));
    public static T Rule => Lazy.Value;

    public virtual bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            dynamicTile.FloorDirection == FloorChangeDirection.None,
            !dynamicTile.HasBlockPathFinding,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            !dynamicTile.HasAnyCreature,
            dynamicTile.Ground is not null);
    }

    public virtual bool CanEnter(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            !dynamicTile.HasAnyCreature,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }

    public virtual bool CanEnter(ITile tile, Location location)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            !dynamicTile.HasAnyCreature,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }
}

public class PlayerEnterTileRule : CreatureEnterTileRule<PlayerEnterTileRule>
{
    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            dynamicTile.FloorDirection == FloorChangeDirection.None,
            !dynamicTile.HasBlockPathFinding,
            !dynamicTile.HasAnyCreature,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null,
            !dynamicTile.HasHole);
    }

    public override bool CanEnter(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        var goingToDifferentFloor = !creature.Location.SameFloorAs(tile.Location);
        var hasMonsterOrNpc = !goingToDifferentFloor &&
                              (dynamicTile.HasCreatureOfType<IMonster>() || dynamicTile.HasCreatureOfType<INpc>());

        return ConditionEvaluation.And(
            !hasMonsterOrNpc,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }

    public override bool CanEnter(ITile tile, Location location)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        var goingToDifferentFloor = !location.SameFloorAs(tile.Location);
        var hasMonsterOrNpc = !goingToDifferentFloor &&
                              (dynamicTile.HasCreatureOfType<IMonster>() || dynamicTile.HasCreatureOfType<INpc>());

        return ConditionEvaluation.And(
            !hasMonsterOrNpc,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }
}

public class MonsterEnterTileRule : CreatureEnterTileRule<MonsterEnterTileRule>
{
    private static bool HasBlockingCreatures(IMonster monster, IDynamicTile dynamicTile)
    {
        if (!dynamicTile.HasAnyCreature) return false;

        if (!monster.Metadata.HasFlag(CreatureFlagAttribute.CanPushCreatures))
            //the tile has a creature and the monster can't push creatures
            return true;

        foreach (var creature in dynamicTile.Creatures)
        {
            if (IsPushable(creature)) continue;

            //the tile has a creature and the monster can't push it
            return true;
        }

        //the tile has no creatures or the monster can push all creatures in the tile
        return false;
    }

    private static bool IsPushable(ICreature creature)
    {
        if (creature is not Monster.Monster monster) return false;
        if (creature is Summon { Master: Player.Player }) return false;
        return monster.IsPushable;
    }

    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (creature is not IMonster monster) return false;

        var hasBlockingCreatures = HasBlockingCreatures(monster, dynamicTile);

        return ConditionEvaluation.And(
            dynamicTile.FloorDirection == FloorChangeDirection.None,
            monster.Metadata.HasFlag(CreatureFlagAttribute.CanPushItems) || !dynamicTile.HasBlockPathFinding,
            !hasBlockingCreatures,
            !dynamicTile.HasTeleport(out _),
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            !dynamicTile.ProtectionZone,
            dynamicTile.Ground is not null);
    }

    public override bool CanEnter(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (creature is not IMonster monster) return false;

        var hasBlockingCreatures = HasBlockingCreatures(monster, dynamicTile);

        return ConditionEvaluation.And(
            !hasBlockingCreatures,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }

    public override bool CanEnter(ITile tile, Location location)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        // For location-based entering, assume no creature, since we don't have the creature here
        return ConditionEvaluation.And(
            !dynamicTile.HasAnyCreature,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }
}

public class MonsterRandomStepEnterTileRule : CreatureEnterTileRule<MonsterRandomStepEnterTileRule>
{
    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (creature is not IMonster monster) return false;

        return ConditionEvaluation.And(
            dynamicTile.FloorDirection == FloorChangeDirection.None,
            monster.Metadata.HasFlag(CreatureFlagAttribute.CanPushItems) || !dynamicTile.HasBlockPathFinding,
            !dynamicTile.HasAnyCreature, // Always ignore tiles with creatures, regardless of push ability
            !dynamicTile.HasTeleport(out _),
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            !dynamicTile.ProtectionZone,
            dynamicTile.Ground is not null);
    }

    public override bool CanEnter(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (creature is not IMonster) return false;

        return ConditionEvaluation.And(
            !dynamicTile.HasAnyCreature, // Always block if any creature present
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }

    public override bool CanEnter(ITile tile, Location location)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            !dynamicTile.HasAnyCreature,
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            dynamicTile.Ground is not null);
    }
}

public class NpcEnterTileRule : CreatureEnterTileRule<NpcEnterTileRule>
{
    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (creature is not INpc npc) return false;
        return base.ShouldIgnore(tile, npc) && npc.SpawnPoint.Location.GetMaxSqmDistance(tile.Location) <= 3;
    }
}