using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.World.Models.Tiles;

public static class TileOperationEvent
{
    public static event Action<ITile, IItem, OperationResultList<IItem>> OnTileChanged;
    public static event Action<ITile> OnTileLoaded;

    public static void OnChanged(ITile tile, IItem thing, OperationResultList<IItem> operation)
    {
        OnTileChanged?.Invoke(tile, thing, operation);
    }

    public static void OnLoaded(ITile tile)
    {
        OnTileLoaded?.Invoke(tile);
    }
}