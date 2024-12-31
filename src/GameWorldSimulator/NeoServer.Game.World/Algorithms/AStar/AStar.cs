using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Game.World.Algorithms.AStar;

public static class AStar
{
    public static (bool Founded, Direction[] Directions) GetPathMatching(
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

    private static int CalculateExtraCost(ICreature creature, Node neighborNode, ITile tile)
    {
        if (neighborNode is not null) return neighborNode.ExtraCost;

        if (!creature && tile is IDynamicTile walkableTile) return Node.GetTileWalkCost(creature, walkableTile);

        return 0;
    }
}