using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Summon;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.World.Algorithms.AStar;

public static class AStar
{
    public static (bool Found, Direction[] Directions) GetPathMatching(
        IMap map,
        ICreature creature,
        Location startpos,
        Location targetPos,
        FindPathParams fpp,
        ITileEnterRule tileEnterRule)
    {
        var pos = startpos;
        var endPos = new Location();
        var bestMatch = 0;
        var nodeList = new NodeList(pos);
        var startPos = pos;
        var sX = Math.Abs(targetPos.X - pos.X);
        var sY = Math.Abs(targetPos.Y - pos.Y);
        Node found = null;

        while (fpp.MaxSearchDist != 0 || nodeList.ClosedNodes < 100)
        {
            var bestNode = nodeList.GetBestNode();
            if (bestNode is null)
            {
                if (found is not null) break;

                return PathFinder.NotFound;
            }

            var x = bestNode.X;
            var y = bestNode.Y;

            pos.X = x;
            pos.Y = y;

            if (AStarCondition.Validate(map, startPos, pos, targetPos, ref bestMatch, fpp))
            {
                found = bestNode;
                endPos = pos;
                if (bestMatch == 0) break;
            }

            var result = AStarNeighbors.GetDirectionsAndNeighbors(bestNode);

            nodeList.CloseNode(bestNode);

            var f = bestNode.F;
            for (var i = 0; i < result.dirCount; ++i)
            {
                pos.X = (ushort)(x + result.neighbors[i, 0]);
                pos.Y = (ushort)(y + result.neighbors[i, 1]);

                if (fpp.CannotWalk(startPos, pos)) continue;
                if (fpp.KeepDistance && !map.IsInRange(startPos, pos, targetPos, fpp)) continue;

                var neighborNode = nodeList.GetNodeByPosition(pos);
                var tile = map[pos];

                if (neighborNode is null &&
                    tileEnterRule != null &&
                    !tileEnterRule.ShouldIgnore(tile, creature)) continue;

                if (neighborNode is null && pos.IsNextTo(targetPos) && !fpp.PushMonsters &&
                    HasPushableMonster(tile)) continue;

                var extraCost = CalculateExtraCost(creature, neighborNode, tile);
                var cost = bestNode.GetMapWalkCost(pos);
                var newF = f + cost + extraCost;

                if (neighborNode is not null && neighborNode.F <= newF) continue;

                if (neighborNode is not null)
                {
                    neighborNode.F = newF;
                    neighborNode.Parent = bestNode;
                    nodeList.OpenNode(neighborNode);
                    continue;
                }

                var dX = Math.Abs(targetPos.X - pos.X);
                var dY = Math.Abs(targetPos.Y - pos.Y);

                neighborNode = nodeList.CreateOpenNode(bestNode, pos.X, pos.Y, newF,
                    ((dX - sX) << 3) + ((dY - sY) << 3) + (Math.Max(dX, dY) << 3), (byte)extraCost);

                if (neighborNode is not null) continue;
                if (found is not null) break;

                return PathFinder.NotFound;
            }
        }

        return found is null ? PathFinder.NotFound : (true, AStarDirections.GetAll(found, startPos, endPos));
    }

    private static bool HasPushableMonster(ITile tile)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        if (!dynamicTile.HasAnyCreature) return false;

        foreach (var creature in dynamicTile.Creatures)
        {
            //if the creature is not a monster, skip it
            if (creature is not Monster monster) continue;

            //if the creature is player's summoned, skip it
            if (creature is Summon { Master: IPlayer }) continue;

            //if a creature can't be pushed, skip it
            if (monster.IsPushable) continue;

            return true;
        }

        return false;
    }

    private static int CalculateExtraCost(ICreature creature, Node neighborNode, ITile tile)
    {
        if (neighborNode is not null) return neighborNode.ExtraCost;

        if (tile is IDynamicTile walkableTile) return Node.GetTileWalkCost(creature, walkableTile);

        return 0;
    }
}