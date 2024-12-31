using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.World.Algorithms.AStar;

namespace NeoServer.Game.World.Map;

public class PathFinder : IPathFinder
{
    public static readonly (bool Founded, Direction[] Directions) FoundedButEmptyDirections =
        (true, Array.Empty<Direction>());

    public static readonly (bool Founded, Direction[] Directions) NotFound = (false, Array.Empty<Direction>());

    public PathFinder(IMap map)
    {
        Map = map;
    }

    public IMap Map { get; set; }

    public (bool Founded, Direction[] Directions) Find(Location startPosition, Location targetPosition,
        FindPathParams fpp)
    {
        return AStar.GetPathMatching(Map, null, startPosition, targetPosition, fpp, null);
    }

    public (bool Founded, Direction[] Directions) Find(ICreature creature, Location target,
        ITileEnterRule tileEnterRule)
    {
        return AStar.GetPathMatching(Map, creature, creature.Location, target, new FindPathParams(true), tileEnterRule);
    }

    public (bool Founded, Direction[] Directions) Find(ICreature creature, Location target, FindPathParams fpp,
        ITileEnterRule tileEnterRule)
    {
        if (creature is not IWalkableCreature walkableCreature) return NotFound;

        if (!creature.Location.SameFloorAs(target)) return NotFound;

        if (!fpp.KeepDistance && creature.Location.IsNextTo(target)) return FoundedButEmptyDirections;

        if (walkableCreature.Speed == 0) return NotFound;

        if (fpp.OneStep) return FindStep(creature, target, fpp, tileEnterRule);

        if (fpp.MaxTargetDist > 1)
        {
            var pathToKeepDistance = FindPathToKeepDistance(creature, target, fpp, tileEnterRule);

            return pathToKeepDistance.Founded
                ? (true, pathToKeepDistance.Directions)
                : AStar.GetPathMatching(Map, creature, creature.Location, target, fpp, tileEnterRule);
        }

        return AStar.GetPathMatching(Map, creature, creature.Location, target, fpp, tileEnterRule);
    }

    public Direction FindRandomStep(ICreature creature, ITileEnterRule rule, Location origin,
        int maxStepsFromOrigin = 1)
    {
        var randomIndex = GameRandom.Random.Next(0, maxValue: 4);

        var directions = new[] { Direction.East, Direction.North, Direction.South, Direction.West };

        for (var i = 0; i < 4; i++)
        {
            randomIndex = randomIndex > 3 ? 0 : randomIndex;
            var direction = directions[randomIndex++];

            if (Map.CanGoToDirection(creature, direction, rule))
            {
                var nextLocation = creature.Location.GetNextLocation(direction);
                if (nextLocation.GetMaxSqmDistance(origin) > maxStepsFromOrigin) continue;

                return direction;
            }
        }

        return Direction.None;
    }

    public Direction FindRandomStep(ICreature creature, ITileEnterRule rule)
    {
        var randomIndex = GameRandom.Random.Next(0, maxValue: 4);

        var directions = new Direction[4] { Direction.East, Direction.North, Direction.South, Direction.West };

        for (var i = 0; i < 4; i++)
        {
            randomIndex = randomIndex > 3 ? 0 : randomIndex;
            var direction = directions[randomIndex++];

            if (Map.CanGoToDirection(creature, direction, rule)) return direction;
        }

        return Direction.None;
    }

    public (bool Founded, Direction[] Directions) FindStep(ICreature creature, Location target, FindPathParams fpp,
        ITileEnterRule tileEnterRule)
    {
        var startLocation = creature.Location;

        var possibleDirections = new[]
            { Direction.East, Direction.South, Direction.West, Direction.North, Direction.NorthEast };

        foreach (var direction in possibleDirections)
            if (startLocation.GetMaxSqmDistance(target) > fpp.MaxTargetDist)
                continue;

        return NotFound;
    }

    public (bool Founded, Direction[] Directions) FindPathToKeepDistance(ICreature creature, Location target,
        FindPathParams fpp, ITileEnterRule tileEnterRule)
    {
        if (fpp.MaxTargetDist <= 1) return FoundedButEmptyDirections;
        var startLocation = creature.Location;

        var currentDistance = startLocation.GetMaxSqmDistance(target);

        if (currentDistance > fpp.MaxTargetDist) return NotFound;

        if (currentDistance == fpp.MaxTargetDist) return FoundedButEmptyDirections;

        var possibleDirections = new[]
        {
            Direction.East, Direction.South, Direction.West, Direction.North, Direction.NorthEast,
            Direction.NorthEast, Direction.SouthEast, Direction.SouthWest
        };

        (Direction, int) directionWeight = (Direction.None, 0);

        var canGoToDirections = new Direction[8];
        var canGoIndex = 0;

        var cannotGoDirectionCount = 0;
        foreach (var direction in possibleDirections)
        {
            var nextLocation = startLocation.GetNextLocation(direction);
            var nextDistance = nextLocation.GetMaxSqmDistance(target);

            var canGoToDirection = Map.CanGoToDirection(creature, direction, tileEnterRule);
            var isDiagonalMovement = startLocation.IsDiagonalMovement(nextLocation);

            if (!canGoToDirection)
            {
                if (!isDiagonalMovement) cannotGoDirectionCount++;
                continue;
            }

            var weight = 0;

            if (isDiagonalMovement && cannotGoDirectionCount < 4)
                continue;

            canGoToDirections[canGoIndex++] = direction;

            if (nextDistance > currentDistance) weight++;
            if (nextLocation.GetSumSqmDistance(target) > startLocation.GetSumSqmDistance(target)) weight++;

            if (weight > directionWeight.Item2)
            {
                directionWeight.Item1 = direction;
                directionWeight.Item2 = weight;
            }
        }

        if (directionWeight.Item1 != Direction.None) return (true, new[] { directionWeight.Item1 });

        if (canGoIndex > 0)
        {
            var randonIndex = GameRandom.Random.Next(0, maxValue: canGoIndex);
            return (true, new[] { canGoToDirections[randonIndex] });
        }

        return FoundedButEmptyDirections;
    }
}