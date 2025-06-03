using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Systems.Services;

public class PlayerUseService : IPlayerUseService
{
    private readonly IMap _map;
    private readonly IWalkToMechanism _walkToMechanism;

    public PlayerUseService(IWalkToMechanism walkToMechanism, IMap map)
    {
        _walkToMechanism = walkToMechanism;
        _map = map;
    }

    public void Use(IPlayer player, IItem item)
    {
        if (Guard.AnyNull(player, item)) return;

        if (!player.Location.IsNextTo(item.Location))
        {
            _walkToMechanism.WalkTo(player, () => player.Use(item), item.Location);
            return;
        }

        player.Use(item);
    }

    public void Use(IPlayer player, IContainer container, byte openAtIndex)
    {
        if (Guard.AnyNull(player, container, openAtIndex)) return;

        if (!player.Location.IsNextTo(container.Location))
        {
            _walkToMechanism.WalkTo(player, () => Use(player, container, openAtIndex), container.Location);
            return;
        }

        player.Use(container, openAtIndex);
    }

    public void Use(IPlayer player, IUsableOn usableItem, IThing usedOn)
    {
        if (Guard.AnyNull(player, usableItem, usedOn)) return;

        if (!player.Location.IsNextTo(usableItem.Location))
        {
            WalkToItem(player, usableItem, usedOn);
            return;
        }

        var itemLocation = usableItem.CanBeMoved
            ? usableItem.Owner?.Location ?? usableItem.Location
            : usableItem.Location;

        if (!itemLocation.IsNextTo(usedOn.Location))
        {
            WalkToTarget(player, usableItem, usedOn);
            return;
        }

        if (usableItem is IUsableOnCreature && usedOn is IDynamicTile tile) usedOn = tile.TopCreatureOnStack;

        UseOn(player, usableItem, usedOn);
    }

    private void WalkToItem(IPlayer player, IUsableOn usableItem, IThing usedOn)
    {
        void PickItemFromGround()
        {
            if (usableItem.Metadata.OnUse is not null &&
                usableItem.Metadata.OnUse.TryGetAttribute<bool>("pickfromground", out var pickFromGround) &&
                pickFromGround)
            {
                if (_map[usableItem.Location] is not IDynamicTile dynamicTile) return;

                var result = player.PickItemFromGround(usableItem, dynamicTile);
                if (result.Failed) return;
            }

            WalkToTarget(player, usableItem, usedOn);
        }

        _walkToMechanism.WalkTo(player, PickItemFromGround, usableItem.Location);
    }

    private void WalkToTarget(IPlayer player, IUsableOn item, IThing destinationThing)
    {
        if (item.Metadata.OnUse is not null &&
            item.Metadata.OnUse.TryGetAttribute<bool>("walktotarget", out var walkToTarget) &&
            walkToTarget)
        {
            _walkToMechanism.WalkTo(player, () => UseOn(player, item, destinationThing), destinationThing.Location);
            return;
        }

        UseOn(player, item, destinationThing);
    }

    private void UseOn(IPlayer player, IUsableOn item, IThing destinationThing)
    {
        switch (destinationThing)
        {
            case ICreature creature:
                player.Use(item, creature);
                break;
            case IItem destinationItem:
                player.Use(item, destinationItem);
                break;
            case ITile destinationTile:
                player.Use(item, destinationTile);
                break;
        }
    }
}