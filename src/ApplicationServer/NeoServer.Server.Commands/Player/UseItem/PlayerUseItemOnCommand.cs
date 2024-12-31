using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemOnCommand : ICommand
{
    private readonly IPlayerUseService _playerUseService;
    private readonly IGameServer _game;
    private readonly HotkeyService _hotkeyService;
    private readonly IScriptGameManager _scriptGameManager;
    private readonly IWalkToMechanism _walkToMechanism;

    public PlayerUseItemOnCommand(
        IGameServer game,
        HotkeyService hotkeyService,
        IPlayerUseService playerUseService,
        IScriptGameManager scriptGameManager,
        IWalkToMechanism walkToMechanism)
    {
        _game = game;
        _hotkeyService = hotkeyService;
        _playerUseService = playerUseService;
        _scriptGameManager = scriptGameManager;
        _walkToMechanism = walkToMechanism;
    }

    public void Execute(IPlayer player, UseItemOnPacket useItemPacket)
    {
        IItem onItem = null;
        ITile onTile = null;

        if (useItemPacket.ToLocation.Type == LocationType.Ground)
        {
            if (_game.Map[useItemPacket.ToLocation] is not { } tile) return;
            onTile = tile;
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

        IThing thingToUse = null;

        if (useItemPacket.Location.IsHotkey)
            thingToUse = _hotkeyService.GetItem(player, useItemPacket.ClientId);

        else
            switch (useItemPacket.Location.Type)
            {
                case LocationType.Ground:
                {
                    if (_game.Map[useItemPacket.Location] is not { } tile) return;
                    thingToUse = tile.TopItemOnStack;
                    break;
                }
                case LocationType.Slot:
                    thingToUse = player.Inventory[useItemPacket.Location.Slot];
                    break;
                case LocationType.Container:
                    thingToUse =
                        player.Containers[useItemPacket.Location.ContainerId][useItemPacket.Location.ContainerSlot];
                    break;
            }

        if (thingToUse is not IUsableOn itemUsableOn) return;

        Action action = null;

        IThing onTarget = !onItem ? onTile : onItem;

        if (_scriptGameManager.HasAction(itemUsableOn))
            action = () => _scriptGameManager.PlayerUseItem(player, player.Location, useItemPacket.ToLocation,
                useItemPacket.ToStackPosition, itemUsableOn, onTarget, useItemPacket.Location.IsHotkey);
        else
            action = () => _playerUseService.Use(player, itemUsableOn, onTarget);

        if (!player.Location.IsNextTo(onTarget.Location))
        {
            _walkToMechanism.WalkTo(player, action, onTarget.Location);
            return;
        }

        action();
    }
}