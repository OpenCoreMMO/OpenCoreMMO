using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Algorithms.AStar;

internal static class AStarCondition
{
    public static bool Validate(IMap map, Location startPos, Location testPos,
        Location targetPos, ref int bestMatchDist, FindPathParams fpp)
    {
        if (!map.IsInRange(startPos, testPos, targetPos, fpp)) return false;

        if (fpp.ClearSight && !SightClear.IsSightClear(map, testPos, targetPos, true)) return false;

        var testDist = Math.Max(targetPos.GetSqmDistanceX(testPos), targetPos.GetSqmDistanceY(testPos));
        if (fpp.MaxTargetDist == 1) return testDist >= fpp.MinTargetDist && testDist <= fpp.MaxTargetDist;

        if (testDist > fpp.MaxTargetDist) return false;
        if (testDist < fpp.MinTargetDist) return false;

        if (testDist == fpp.MaxTargetDist)
        {
            bestMatchDist = 0;
            return true;
        }

        if (testDist <= bestMatchDist) return false;

        //not quite what we want, but the best so far
        bestMatchDist = testDist;
        return true;
    }
}