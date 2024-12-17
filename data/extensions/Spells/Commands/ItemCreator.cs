using System;
using System.Collections.Generic;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Items.Factories;

namespace NeoServer.Extensions.Spells.Commands;

public class ItemCreator : CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;
        if (Params?.Length == 0) return false;

        var amount = Params.Length > 1 && byte.TryParse(Params[1].ToString(), out var count)
            ? count > 100 ? 100 : count
            : 1;

        var item = GetItem(actor, amount);

        if (item is null) return false;

        var result = CreateItem(actor, item);

        if (!result)
            error = InvalidOperation.NotEnoughRoom;

        return result;
    }

    private IItem GetItem(ICombatActor actor, int amount)
    {
        if (ushort.TryParse(Params[0].ToString(), out var typeId))
            return ItemFactory.Instance.Create(typeId, actor.Location,
                new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } });

        var item = ItemFactory.Instance.Create(Params[0].ToString(), actor.Location,
            new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } });

        return item;
    }

    private bool CreateItem(ICombatActor actor, IItem item)
    {
        if (item is null) return false;

        if (!item.IsPickupable && actor.Tile is { } tile && tile.AddItem(item).Succeeded)
        {
            item.Decay.StartDecay();
            return true;
        }

        if (actor is not IPlayer player) return false;

        var itemCanBeAddedToSlot = item.Metadata.BodyPosition != Slot.None &&
                                   item.Metadata.BodyPosition == Slot.TwoHanded && !player.Inventory.HasShield &&
                                   !player.Inventory.IsUsingWeapon
                                   && player.Inventory[item.Metadata.BodyPosition] is null;

        if (itemCanBeAddedToSlot)
            return player.Inventory.AddItem(item).Succeeded;

        var result = player.Inventory.AddItem(item, Slot.Backpack);

        if (result.Failed && actor.Tile is { } playerTile && playerTile.AddItem(item).Succeeded)
        {
            item.Decay.StartDecay();
            return true;
        }

        return result.Succeeded;
    }
}