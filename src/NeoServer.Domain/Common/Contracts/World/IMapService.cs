using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Common.Contracts.World;

public interface IMapService
{
    void ReplaceGround(Location.Structs.Location location, IGround ground);
    ITile GetFinalTile(Location.Structs.Location location);

    bool GetNeighbourAvailableTile(Location.Structs.Location location, ICreature creature, ITileEnterRule rule,
        out ITile foundTile);
}