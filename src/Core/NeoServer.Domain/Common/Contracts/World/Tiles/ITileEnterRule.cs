using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.World.Tiles;

public interface ITileEnterRule
{
    bool ShouldIgnore(ITile tile, ICreature creature);
    bool CanEnter(ITile tile, ICreature creature);
    bool CanEnter(ITile tile, Location.Structs.Location location);
}