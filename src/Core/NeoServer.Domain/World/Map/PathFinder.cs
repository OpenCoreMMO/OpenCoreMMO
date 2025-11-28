using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Algorithms;
using NeoServer.Domain.World.Algorithms.AStar;

namespace NeoServer.Domain.World.Map;

public class PathFinder(IMap map) : IPathFinder
{
    public static readonly (bool Found, Direction[] Directions) FoundedButEmptyDirections =
        (true, []);

    public static readonly (bool Found, Direction[] Directions) NotFound = (false, []);

    public (bool Found, Direction[] Directions) Find(Location startPosition, Location targetPosition,
        FindPathParams fpp)
    {
        return AStar.GetPathMatching(map, null, startPosition, targetPosition, fpp, null);
    }

    public (bool Found, Direction[] Directions) Find(ICreature creature, Location target,
        ITileEnterRule tileEnterRule)
    {
        return AStar.GetPathMatching(map, creature, creature.Location, target, new FindPathParams(true), tileEnterRule);
    }

    public (bool Found, Direction[] Directions) Find(ICreature creature, Location target, FindPathParams fpp,
        ITileEnterRule tileEnterRule)
    {
        if (creature is not IWalkableCreature walkableCreature) return NotFound;

        if (!creature.Location.SameFloorAs(target)) return NotFound;

        if (!fpp.KeepDistance && creature.Location.IsNextTo(target)) return FoundedButEmptyDirections;
        
        if (fpp.MaxTargetDist > 1)
        {
            var pathToKeepDistance = FindPathToKeepDistance(creature, target, fpp, tileEnterRule);

            return pathToKeepDistance.Found
                ? (true, pathToKeepDistance.Directions)
                : AStar.GetPathMatching(map, creature, creature.Location, target, fpp, tileEnterRule);
        }

        return AStar.GetPathMatching(map, creature, creature.Location, target, fpp, tileEnterRule);
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

            if (map.CanGoToDirection(creature, direction, rule))
            {
                var nextLocation = creature.Location.GetNextLocation(direction);
                if (nextLocation.GetMaxSqmDistance(origin) > maxStepsFromOrigin) continue;

                return direction;
            }
        }

        return Direction.None;
    }

    public Direction FindRandomStep(ICreature creature, ITileEnterRule rule, bool allowDiagonal = false)
    {
        Span<Direction> directions = allowDiagonal
            ?
            [
                Direction.East, Direction.North, Direction.South, Direction.West, Direction.NorthEast,
                Direction.NorthWest, Direction.SouthEast, Direction.SouthWest
            ]
            : [Direction.East, Direction.North, Direction.South, Direction.West];

        var randomIndex = GameRandom.Random.Next(0, maxValue: directions.Length);

        for (var i = 0; i < directions.Length; i++)
        {
            randomIndex = randomIndex >= directions.Length ? 0 : randomIndex;

            var direction = directions[randomIndex++];
            if (map.CanGoToDirection(creature, direction, rule)) return direction;
        }

        return Direction.None;
    }
    
    public (bool Found, Direction[] Directions) FindPathToKeepDistance(
        ICreature creature,
        Location target,
        FindPathParams fpp,
        ITileEnterRule tileEnterRule)
    {
        var start = creature.Location;
        var currentDistance = start.GetMaxSqmDistance(target);

        // Already at the desired distance — no need to move
        if (currentDistance == fpp.MaxTargetDist)
            if (!fpp.ClearSight || SightClear.IsSightClear(map, start, target, false))
                return FoundedButEmptyDirections;

        var shouldMoveCloser = currentDistance > fpp.MaxTargetDist;
        var shouldMoveFarther = !shouldMoveCloser;

        var allDirections = new[]
        {
            // Cardinal directions first
            Direction.East, Direction.South, Direction.West, Direction.North,
            // Diagonal directions (lower preference)
            Direction.NorthEast, Direction.NorthWest, Direction.SouthEast, Direction.SouthWest
        };

        (Direction BestDirection, int Score) best = (Direction.None, int.MinValue);
        var fallbackOptions = new List<Direction>();

        foreach (var direction in allDirections)
        {
            var next = start.GetNextLocation(direction);
            if (!map.CanGoToDirection(creature, direction, tileEnterRule))
                continue;

            // If clear sight is required, skip candidates that do not have a sight from 'next'
            if (fpp.ClearSight && !SightClear.IsSightClear(map, next, target, false))
                continue;

            var nextDistance = next.GetMaxSqmDistance(target);
            var isDiagonal = start.IsDiagonalMovement(next);

            // Calculate a score based on how well the move matches the goal
            var score = 0;

            if (shouldMoveCloser && nextDistance < currentDistance)
                score++;

            if (shouldMoveFarther && nextDistance > currentDistance)
                score++;

            // Add an extra point if the Manhattan distance also improves
            var currentManhattan = start.GetSumSqmDistance(target);
            var nextManhattan = next.GetSumSqmDistance(target);

            if (shouldMoveCloser && nextManhattan < currentManhattan)
                score++;

            if (shouldMoveFarther && nextManhattan > currentManhattan)
                score++;

            // Penalize diagonal moves to favor cardinal movement
            if (isDiagonal)
                score--;

            // Track best direction so far
            if (score > best.Score)
                best = (direction, score);

            if (score >= 0)
                fallbackOptions.Add(direction);
        }

        // Return best move, or fallback, or nothing
        if (best.BestDirection != Direction.None)
            return (true, [best.BestDirection]);

        if (fallbackOptions.Count > 0)
        {
            var randomDirection = fallbackOptions[GameRandom.Random.Next(0, maxValue: fallbackOptions.Count)];
            return (true, [randomDirection]);
        }

        return NotFound;
    }
}