using System;
using NeoServer.Game.Combat.Services.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Items.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;
using Serilog;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemOnCreatureCommand : ICommand
{
    private readonly IGameServer _game;
    private readonly HotkeyService _hotKeyService;
    private readonly IPlayerUseService _playerUseService;
    private readonly IScriptManager _scriptManager;
    private readonly IWalkToMechanism _walkToMechanism;
    private readonly ItemUseValidation _itemUseValidation;
    private readonly SpellService _spellService;
    private readonly ILogger _logger;
    private readonly GameConfiguration _gameConfiguration;

    public PlayerUseItemOnCreatureCommand(
        IGameServer game,
        HotkeyService hotKeyCache,
        IPlayerUseService playerUseService,
        IScriptManager scriptManager,
        IWalkToMechanism walkToMechanism,
        ItemUseValidation itemUseValidation,
        SpellService spellService,
        ILogger logger,
        GameConfiguration gameConfiguration)
    {
        _game = game;
        _hotKeyService = hotKeyCache;
        _playerUseService = playerUseService;
        _scriptManager = scriptManager;
        _walkToMechanism = walkToMechanism;
        _itemUseValidation = itemUseValidation;
        _spellService = spellService;
        _logger = logger;
        _gameConfiguration = gameConfiguration;
    }

    public void Execute(IPlayer player, UseItemOnCreaturePacket useItemPacket)
    {
        if (!_game.CreatureManager.TryGetCreature(useItemPacket.CreatureId, out var targetCreature)) return;

        var itemToUse = GetItem(player, useItemPacket);
        
        Action action = null;

        var itemUseValidationResult =
            _itemUseValidation.CanUse(itemToUse, player, targetCreature, new ItemUseValidationParam(true, true));

        if (itemUseValidationResult.Failed)
        {
            OperationFailService.Send(player, itemUseValidationResult.Reason, EffectT.Puff);
            return;
        }

        if (itemToUse is IRune rune)
        {
            action = () => UseRune(player, useItemPacket, rune, targetCreature);
        }

        if (action is null)
        {
            if (itemToUse is not IUsableOn usable) return;

            if (_scriptManager.Actions.HasAction(usable))
            {
                action = () => _scriptManager.Actions.UseItem(player, player.Location, useItemPacket.FromStackPosition,
                    0,
                    usable, targetCreature);
            }
            else
                action = () => _playerUseService.Use(player, usable, targetCreature);
        }

        if (!player.Location.IsNextTo(itemToUse.Location))
        {
            _walkToMechanism.WalkTo(player, action, itemToUse.Location);
            return;
        }

        action?.Invoke();
    }

    private void UseRune(IPlayer player, UseItemOnCreaturePacket useItemPacket, IRune rune, ICreature targetCreature)
    {
        var result = rune.CanBeCastBy(player, targetCreature);

        if (result.Failed)
        {
            OperationFailService.Send(player, result.Reason, EffectT.Puff);
            return;
        }

        var spell = rune.Spell;

        if (spell is null)
        {
            _logger.Warning("Rune {Name} has no spell implemented", rune.Name);
            return;
        }

        var castResult = _spellService.Cast(player, targetCreature, spell, useItemPacket.FromLocation.IsHotkey);
        if (!castResult)
        {
            return;
        }

        rune.PostUse(!_gameConfiguration.InfiniteRuneCharges);
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