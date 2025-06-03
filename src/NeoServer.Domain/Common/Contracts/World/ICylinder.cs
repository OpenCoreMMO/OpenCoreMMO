using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.World;

public interface ICylinder
{
    ICylinderSpectator[] TileSpectators { get; }
    ITile FromTile { get; }
    ITile ToTile { get; }
    IThing Thing { get; }
    Operation Operation { get; }

    bool IsTeleport { get; }
}

public interface ICylinderSpectator
{
    ICreature Spectator { get; }
    byte FromStackPosition { get; set; }
    byte ToStackPosition { get; set; }
}