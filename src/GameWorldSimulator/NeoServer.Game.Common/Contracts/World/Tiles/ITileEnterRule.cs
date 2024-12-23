using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.World.Tiles;

public interface ITileEnterRule
{
    bool ShouldIgnore(ITile tile, ICreature creature);
    bool CanEnter(ITile tile, ICreature creature);
    bool CanEnter(ITile tile, Location.Structs.Location location);
}