using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player;

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
        {
            return true;
        }

        foreach (var creature in dynamicTile.Creatures)
        {
            if (IsPushable(creature)) continue;
            return true;
        }

        return true;
    }

    private static bool IsPushable(ICreature creature)
    {
        if (creature is not NeoServer.Domain.Creatures.Monster.Monster monster) return false;
        if (creature is Summon { Master: NeoServer.Domain.Creatures.Player.Player }) return false;
        if (monster.Metadata.HasFlag(CreatureFlagAttribute.CanPushCreatures)) return false;
        return true;
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

public class NpcEnterTileRule : CreatureEnterTileRule<NpcEnterTileRule>
{
    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (creature is not INpc npc) return false;
        return base.ShouldIgnore(tile, npc) && npc.SpawnPoint.Location.GetMaxSqmDistance(tile.Location) <= 3;
    }
}