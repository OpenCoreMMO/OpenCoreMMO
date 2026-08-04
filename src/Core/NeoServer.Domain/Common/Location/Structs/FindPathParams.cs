namespace NeoServer.Domain.Common.Location.Structs;

public class FindPathParams
{
    public FindPathParams(bool fullPathSearch, bool clearSight, bool allowDiagonal, bool keepDistance,
        int maxSearchDist, int minTargetDist, int maxTargetDist)
    {
        FullPathSearch = fullPathSearch;
        ClearSight = clearSight;
        AllowDiagonal = allowDiagonal;
        KeepDistance = keepDistance;
        MaxSearchDist = maxSearchDist;
        MinTargetDist = minTargetDist;
        MaxTargetDist = maxTargetDist;
    }

    public FindPathParams(bool useDefault)
    {
        FullPathSearch = false;
        ClearSight = false;
        AllowDiagonal = false;
        KeepDistance = false;
        MaxSearchDist = 0;
        MinTargetDist = 0;
        MaxTargetDist = 0;

        if (useDefault)
        {
            FullPathSearch = true;
            ClearSight = true;
            AllowDiagonal = true;
            KeepDistance = false;
            MaxSearchDist = 12;
            MinTargetDist = 1;
            MaxTargetDist = 1;
        }
    }

    public static FindPathParams EscapeParams => new(false, true, false, true, 12, 1, 12);

    public bool FullPathSearch { get; set; }
    public bool ClearSight { get; set; }
    public bool AllowDiagonal { get; set; }
    public bool KeepDistance { get; set; }
    public int MaxSearchDist { get; set; }
    public int MinTargetDist { get; set; }
    public int MaxTargetDist { get; set; }
    public bool PushMonsters { get; set; }

    public bool CannotWalk(Location startPos, Location pos)
    {
        return MaxSearchDist != 0 && (startPos.GetSqmDistanceX(pos) > MaxSearchDist ||
                                      startPos.GetSqmDistanceY(pos) > MaxSearchDist);
    }
}