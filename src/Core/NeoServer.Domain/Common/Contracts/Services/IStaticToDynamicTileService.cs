using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IStaticToDynamicTileService
{
    ITile TransformIntoDynamicTile(ITile tile);
}