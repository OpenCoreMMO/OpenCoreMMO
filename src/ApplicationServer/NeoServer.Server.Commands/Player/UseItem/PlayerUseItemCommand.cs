using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemCommand : ICommand
{
    private readonly ItemFinderService _itemFinderService;
    private readonly PlayerOpenDepotCommand _playerOpenDepotCommand;
    private readonly IPlayerUseService _playerUseService;
    private readonly IScriptManager _scriptManager;
    private readonly IWalkToMechanism _walkToMechanism;

    public PlayerUseItemCommand(
        IPlayerUseService playerUseService,
        PlayerOpenDepotCommand playerOpenDepotCommand,
        ItemFinderService itemFinderService,
        IScriptManager scriptManager,
        IWalkToMechanism walkToMechanism)
    {
        _playerUseService = playerUseService;
        _playerOpenDepotCommand = playerOpenDepotCommand;
        _itemFinderService = itemFinderService;
        _scriptManager = scriptManager;
        _walkToMechanism = walkToMechanism;
    }

    public void Execute(IPlayer player, UseItemPacket useItemPacket)
    {
        var item = _itemFinderService.Find(player, useItemPacket.Location, useItemPacket.ClientId);
            
        Action action;

        switch (item)
        {
            case null:
                return;
            case IDepot depot:
                action = () => _playerOpenDepotCommand.Execute(player, depot, useItemPacket);
                break;
            case IContainer container:
                action = () => _playerUseService.Use(player, container, useItemPacket.Index);
                break;
            case IUsableOn usableOn:
                action = () => _playerUseService.Use(player, usableOn, player);
                break;
            default:
                action = () => _playerUseService.Use(player, item);
                break;
        }

        if (_scriptManager.Actions.HasAction(item))
            action = () => _scriptManager.Actions.UseItem(player, useItemPacket.Location, useItemPacket.StackPosition,
                useItemPacket.Index, item);

        if (!player.Location.IsNextTo(item.Location))
        {
            _walkToMechanism.WalkTo(player, action, item.Location);
            return;
        }

        action();
    }
}