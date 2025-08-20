using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Depot;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Player.UseItem.OpenLocker;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemCommand(
    PlayerOpenDepotCommand openDepotCommand,
    IScriptManager scriptManager,
    IWalkToMechanism walkToMechanism,
    IPlayerUseService playerUseService,
    PlayerOpenLockerCommand playerOpenLockerCommand,
    ItemFinderService itemFinderService) : ICommand
{
    public void Execute(IPlayer player, UseItemPacket useItemPacket)
    {
        var item = itemFinderService.Find(player, useItemPacket.Location, useItemPacket.ClientId);

        Action action;

        switch (item)
        {
            case null:
                return;
            case Locker locker:
                action = () => playerOpenLockerCommand.Execute(player, locker, useItemPacket);
                break;
            case IContainer container:

                if (container.Owner is Locker && container.ServerId is 2594) //depot chest
                {
                    action = () => openDepotCommand.Execute(player, container, useItemPacket);
                    break;
                }

                action = () => playerUseService.Use(player, container, useItemPacket.Index);
                break;
            case IUsableOn usableOn:
                action = () => playerUseService.Use(player, usableOn, player);
                break;
            default:
                action = () => playerUseService.Use(player, item);
                break;
        }

        if (scriptManager.Actions.HasAction(item))
            action = () => scriptManager.Actions.UseItem(player, useItemPacket.Location, useItemPacket.StackPosition,
                useItemPacket.Index, item);

        if (!player.Location.IsNextTo(item.Location))
        {
            walkToMechanism.WalkTo(player, action, item.Location);
            return;
        }

        action();
    }
}