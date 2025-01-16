using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Location;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;
using System;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemOnCreatureCommand : ICommand
{
    private readonly IPlayerUseService _playerUseService;
    private readonly IGameServer _game;
    private readonly HotkeyService _hotKeyService;
    private readonly IScriptGameManager _scriptGameManager;
    private readonly IWalkToMechanism _walkToMechanism;

    public PlayerUseItemOnCreatureCommand(
        IGameServer game,
        HotkeyService hotKeyCache,
        IPlayerUseService playerUseService,
        IScriptGameManager scriptGameManager,
        IWalkToMechanism walkToMechanism)
    {
        _game = game;
        _hotKeyService = hotKeyCache;
        _playerUseService = playerUseService;
        _scriptGameManager = scriptGameManager;
        _walkToMechanism = walkToMechanism;
    }

    public void Execute(IPlayer player, UseItemOnCreaturePacket useItemPacket)
    {
        if (!_game.CreatureManager.TryGetCreature(useItemPacket.CreatureId, out var creature)) return;

        var itemToUse = GetItem(player, useItemPacket);

        if (itemToUse is not IUsableOn useableOn) return;

        Action action = null;

        if (_scriptGameManager.Actions.HasAction(useableOn))
            action = () => _scriptGameManager.Actions.PlayerUseItem(player, player.Location, useItemPacket.FromStackPosition, 0,
                useableOn, creature);
        else
            action = () => _playerUseService.Use(player, useableOn, creature);

        if (!player.Location.IsNextTo(useableOn.Location))
        {
            _walkToMechanism.WalkTo(player, action, useableOn.Location);
            return;
        }

        action();
    }

    private IThing GetItem(IPlayer player, UseItemOnCreaturePacket useItemPacket)
    {
        if (useItemPacket.FromLocation.IsHotkey) return _hotKeyService.GetItem(player, useItemPacket.ClientId);

        if (useItemPacket.FromLocation.Type == LocationType.Ground)
        {
            if (_game.Map[useItemPacket.FromLocation] is not { } tile) return null;
            return tile.TopItemOnStack;
        }

        if (useItemPacket.FromLocation.Type == LocationType.Slot)
        {
            if (player.Inventory[useItemPacket.FromLocation.Slot] is null) return null;
            return player.Inventory[useItemPacket.FromLocation.Slot];
        }

        if (useItemPacket.FromLocation.Type == LocationType.Container)
        {
            if (player.Containers[useItemPacket.FromLocation.ContainerId][useItemPacket.FromLocation.ContainerSlot]
                is not IThing thing) return null;
            return thing;
        }

        return null;
    }
}