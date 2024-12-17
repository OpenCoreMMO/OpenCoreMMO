using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;

namespace NeoServer.Game.Creatures;

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
            !dynamicTile.HasCreature,
            dynamicTile.Ground is not null);
    }

    public virtual bool CanEnter(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;

        return ConditionEvaluation.And(
            !dynamicTile.HasCreature,
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
            !dynamicTile.HasCreature,
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
}

public class MonsterEnterTileRule : CreatureEnterTileRule<MonsterEnterTileRule>
{
    public override bool ShouldIgnore(ITile tile, ICreature creature)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (creature is not IMonster monster) return false;

        return ConditionEvaluation.And(
            dynamicTile.FloorDirection == FloorChangeDirection.None,
            monster.Metadata.HasFlag(CreatureFlagAttribute.CanPushItems) || !dynamicTile.HasBlockPathFinding,
            !dynamicTile.HasCreature,
            !dynamicTile.HasTeleport(out _),
            !dynamicTile.HasFlag(TileFlags.Unpassable),
            !dynamicTile.ProtectionZone,
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