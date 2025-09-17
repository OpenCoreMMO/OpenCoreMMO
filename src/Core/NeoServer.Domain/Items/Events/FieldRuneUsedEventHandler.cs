using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.UsableItems.Runes;

namespace NeoServer.Domain.Items.Events;

public class FieldRuneUsedEventHandler : IGameEventHandler
{
    private readonly IAreaEffectStore _areaEffectStore;
    private readonly IItemFactory itemFactory;
    private readonly IMap map;

    public FieldRuneUsedEventHandler(IMap map, IItemFactory itemFactory, IAreaEffectStore areaEffectStore)
    {
        this.map = map;
        this.itemFactory = itemFactory;
        _areaEffectStore = areaEffectStore;
    }

    public void Execute(ICreature usedBy, IDynamicTile onTile, IUsableOnTile item)
    {
        if (item is not FieldRune rune) return;

        if (!string.IsNullOrWhiteSpace(rune.Area))
        {
            var template = _areaEffectStore.Get(rune.Area);
            foreach (var coordinate in AreaEffect.Create(onTile.Location, template))
            {
                var location = coordinate.Location;
                var field = itemFactory.Create(rune.Field, location, null);

                if (map[location] is not IDynamicTile tile) continue;

                tile.AddItem(field);

                CauseDamageToCreaturesOnTile(field, tile);
            }
        }
        else
        {
            var field = itemFactory.Create(rune.Field, onTile.Location, null);
            onTile.AddItem(field);

            CauseDamageToCreaturesOnTile(field, onTile);
        }
    }

    public void CauseDamageToCreaturesOnTile(IItem item, ITile tile)
    {
        if (item is not MagicField field) return;
        if (tile is not IDynamicTile onTile) return;

        if (!onTile.HasAnyCreature) return;

        foreach (var creature in onTile.Creatures.ToArray()) field.CauseDamage(creature);
    }
}