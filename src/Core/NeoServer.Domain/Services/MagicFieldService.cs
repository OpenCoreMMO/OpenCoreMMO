using NeoServer.Domain.Combat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Services;

public class MagicFieldService(IMap map, IItemFactory itemFactory, PvPConfiguration pvpConfiguration)
{
    public Result<MagicField> AddToGround(ICreature actor, Location location, MagicFieldType magicFieldType)
    {
        if (location == Location.Zero) return Result<MagicField>.NotPossible;
        var tile = map.GetTile(location);

        return AddToGround(actor, tile, magicFieldType);
    }

    public Result<MagicField> AddToGround(ICreature actor, ITile tile, MagicFieldType magicFieldType)
    {
        if (tile is not IDynamicTile dynamicTile) return Result<MagicField>.NotPossible;

        if (tile.ProtectionZone || actor.Tile.ProtectionZone)
        {
            return Result<MagicField>.Fail(InvalidOperation.NotPermittedInProtectionZone);
        }

        var isNonPvpField = IsNonPvpField(actor, tile);
        var fireFieldId = (ushort)(isNonPvpField ? 1500 : 1492);

        var field = magicFieldType switch
        {
            MagicFieldType.Poison => itemFactory.Create(1490, tile.Location),
            MagicFieldType.Energy => itemFactory.Create(1491, tile.Location),
            MagicFieldType.Fire => itemFactory.Create(fireFieldId, tile.Location),
            _ => null
        };

        if (field is not MagicField magicField)
        {
            return Result<MagicField>.NotPossible;
        }

        magicField.Creator = actor;

        dynamicTile.RemoveItem(magicField.Metadata.Group);

        dynamicTile.AddItem(magicField);

        if (isNonPvpField || dynamicTile.CreaturesCount == 0) return Result<MagicField>.Ok(magicField);

        foreach (var creature in dynamicTile.Creatures)
        {
            magicField.CauseDamage(creature);
        }

        return Result<MagicField>.Ok(magicField);
    }

    private bool IsNonPvpField(ICreature actor, ITile target)
    {
        if (pvpConfiguration.PvpType != PvpType.OptionalPvP) return false;

        if (actor is not (IPlayer or ISummon { Master: IPlayer })) return false;

        return actor.Tile.PvpZone && target.PvpZone;
    }
}