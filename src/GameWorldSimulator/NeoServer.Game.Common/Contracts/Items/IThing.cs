using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Location;

namespace NeoServer.Game.Common.Contracts.Items;

public interface IThing : IUsable
{
    string Name { get; }

    public byte Amount => 1;

    Location.Structs.Location Location { get; }

    string GetLookText(bool isClose = false, bool showInternalDetails = false);

    public bool IsCloseTo(IThing thing)
    {
        if (Location.Type is not LocationType.Ground &&
            this is IItem { CanBeMoved: true } item)
            return item.Owner?.Location.IsNextTo(thing.Location) ?? false;
        return Location.IsNextTo(thing.Location);
    }

    void SetNewLocation(Location.Structs.Location location);

    static bool operator !(IThing thing)
    {
        return thing is null;
    }
}