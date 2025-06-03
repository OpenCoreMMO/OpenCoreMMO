using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IItemService
{
    IItem Transform(ITile tile, ushort fromItemId, ushort toItemId);
    IItem Transform(Location.Structs.Location location, ushort fromItemId, ushort toItemId);
    IItem Create(Location.Structs.Location location, ushort id);
}