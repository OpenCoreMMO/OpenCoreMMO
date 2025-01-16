using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemOnCommand : ICommand
{
    private readonly IPlayerUseService _playerUseService;
    private readonly IGameServer _game;
    private readonly IScriptGameManager _scriptGameManager;
    private readonly IWalkToMechanism _walkToMechanism;
    private readonly ItemFinderService _itemFinder;

    public PlayerUseItemOnCommand(
        IGameServer game,
        IPlayerUseService playerUseService,
        IScriptGameManager scriptGameManager,
        IWalkToMechanism walkToMechanism,
        ItemFinderService itemFinder)
    {
        _game = game;
        _playerUseService = playerUseService;
        _scriptGameManager = scriptGameManager;
        _walkToMechanism = walkToMechanism;
        _itemFinder = itemFinder;
    }

    public void Execute(IPlayer player, UseItemOnPacket useItemPacket)
    {
        IItem onItem = null;
        ITile onTile = null;

        if (useItemPacket.ToLocation.Type == LocationType.Ground)
        {
            if (_game.Map[useItemPacket.ToLocation] is not { } tile) return;
            onTile = tile is IStaticTile staticTile ? staticTile.CreateClone(useItemPacket.ToLocation) : tile;
        }

        if (useItemPacket.ToLocation.Type == LocationType.Slot)
        {
            if (player.Inventory[useItemPacket.ToLocation.Slot] is null) return;
            onItem = player.Inventory[useItemPacket.ToLocation.Slot];
        }

        if (useItemPacket.ToLocation.Type == LocationType.Container)
        {
            if (player.Containers[useItemPacket.ToLocation.ContainerId][useItemPacket.ToLocation.ContainerSlot] is
                not { } item) return;
            onItem = item;
        }

        if (onItem is null && onTile is null) return;

        var thingToUse = _itemFinder.Find(player, useItemPacket.Location, useItemPacket.ClientId);

        Action action;

        IThing onTarget = !onItem ? onTile : onItem;

        if (_scriptGameManager.Actions.HasAction(thingToUse))
        {
            action = () => _scriptGameManager.Actions.PlayerUseItem(player, player.Location, useItemPacket.ToLocation,
                useItemPacket.ToStackPosition, thingToUse, onTarget, useItemPacket.Location.IsHotkey);
        }
        else
        {
            if (thingToUse is not IUsableOn itemUsableOn) return;
            action = () => _playerUseService.Use(player, itemUsableOn, onTarget);
        }
            

        if (!player.Location.IsNextTo(onTarget.Location == Location.Zero ? useItemPacket.ToLocation : onTarget.Location))
        {
            _walkToMechanism.WalkTo(player, action, onTarget.Location);
            return;
        }

        action();
    }
}