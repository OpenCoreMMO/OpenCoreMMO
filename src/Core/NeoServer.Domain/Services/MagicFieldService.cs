using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Services;

public class MagicFieldService(IMap map, IItemFactory itemFactory)
{
    public MagicField AddToGround(Location location, MagicFieldType magicFieldType)
    {
        if (location == Location.Zero) return null;
        var tile = map.GetTile(location);

        return AddToGround(tile, magicFieldType);
    }

    public MagicField AddToGround(ITile tile, MagicFieldType magicFieldType)
    {
        if (tile is not IDynamicTile dynamicTile) return null;

        var field = magicFieldType switch
        {
            MagicFieldType.Poison => itemFactory.Create(1490, tile.Location),
            MagicFieldType.Energy => itemFactory.Create(1491, tile.Location),
            MagicFieldType.Fire => itemFactory.Create(1492, tile.Location),
            _ => null
        };

        if (field is not MagicField magicField)
        {
            return null;
        }

        dynamicTile.AddItem(magicField);

        if (dynamicTile.CreaturesCount == 0) return magicField;

        foreach (var creature in dynamicTile.Creatures)
        {
            magicField.CauseDamage(creature);
        }

        return magicField;
    }
}