using System;
using NeoServer.Game.Combat.Services.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Creatures.Services;
using NeoServer.Game.Items.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;
using Serilog;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerUseItemOnCommand : ICommand
{
    private readonly IGameServer _game;
    private readonly ItemFinderService _itemFinder;
    private readonly SpellService _spellService;
    private readonly GameConfiguration _gameConfiguration;
    private readonly ItemUseValidation _itemUseValidation;
    private readonly IItemMovementService _itemMovementService;
    private readonly ILogger _logger;
    private readonly IPlayerUseService _playerUseService;
    private readonly IScriptManager _scriptManager;
    private readonly IWalkToMechanism _walkToMechanism;

    public PlayerUseItemOnCommand(
        IGameServer game,
        IPlayerUseService playerUseService,
        IScriptManager scriptManager,
        IWalkToMechanism walkToMechanism,
        ItemFinderService itemFinder,
        SpellService spellService,
        GameConfiguration gameConfiguration,
        ItemUseValidation itemUseValidation,
        IItemMovementService itemMovementService,
        ILogger logger)
    {
        _game = game;
        _playerUseService = playerUseService;
        _scriptManager = scriptManager;
        _walkToMechanism = walkToMechanism;
        _itemFinder = itemFinder;
        _spellService = spellService;
        _gameConfiguration = gameConfiguration;
        _itemUseValidation = itemUseValidation;
        _itemMovementService = itemMovementService;
        _logger = logger;
    }

    public void Execute(IPlayer player, UseItemOnPacket useItemPacket)
    {
        IItem onItem = null;
        ITile onTile = null;

        var isHotkey = useItemPacket.Location.IsHotkey;

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

        IThing onTarget = !onItem ? onTile : onItem;

        UseItem(player, useItemPacket, thingToUse, onTarget, isHotkey);
    }

    private void UseItem(IPlayer player, UseItemOnPacket useItemPacket, IItem thingToUse, IThing onTarget,
        bool isHotkey)
    {
        if (!thingToUse.IsCloseTo(player))
        {
            _walkToMechanism.WalkTo(player, () => UseItem(player, useItemPacket, thingToUse, onTarget, isHotkey),
                thingToUse.Location);
            return;
        }

        if (!thingToUse.AllowFarUse && thingToUse.Location.Type == LocationType.Ground && thingToUse.IsCloseTo(player) && !onTarget.IsCloseTo(player))
        {
            var fromTile = _game.Map[thingToUse.Location] as IDynamicTile;
            var result = _itemMovementService.Move(player, thingToUse, fromTile, player.Inventory.BackpackSlot, 1, 0, 0, walkTo: false);
            
            if (result.Failed)
            {
                OperationFailService.Send(player, result.Error);
                return;
            }
        }
        
        if (!onTarget.IsCloseTo(player) && !thingToUse.AllowFarUse)
        {
            _walkToMechanism.WalkTo(player, () => UseItem(player, useItemPacket, thingToUse, onTarget, isHotkey),
                onTarget.Location);
            return;
        }

        var itemUseValidationResult =
            _itemUseValidation.CanUse(thingToUse, player, onTarget, new ItemUseValidationParam(true, true));

        if (itemUseValidationResult.Failed)
        {
            if (itemUseValidationResult.Reason != InvalidOperation.TooFar)
            {
                OperationFailService.Send(player, itemUseValidationResult.Reason, EffectT.Puff);
                return;
            }
        }

        if (thingToUse is IRune rune)
        {
            UseRune(rune, player, onTarget, isHotkey);
        }
        else
        {
            if (_scriptManager.Actions.HasAction(thingToUse))
            {
                _scriptManager.Actions.UseItem(player, player.Location, useItemPacket.ToLocation,
                    useItemPacket.ToStackPosition, thingToUse, onTarget, isHotkey);
            }
            else
            {
                if (thingToUse is not IUsableOn itemUsableOn) return;
                _playerUseService.Use(player, itemUsableOn, onTarget);
            }
        }

    }
    
    private void UseRune(IRune rune, IPlayer player, IThing target, bool isHotkey)
    {
        var result = rune.CanBeCastBy(player, target);

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

        var castResult = _spellService.Cast(player, target, spell, isHotkey);
        if (!castResult)
        {
            return;
        }

        rune.PostUse(!_gameConfiguration.InfiniteRuneCharges);
    }
}