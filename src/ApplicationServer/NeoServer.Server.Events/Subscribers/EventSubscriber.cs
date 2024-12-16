
using System;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Items.Events;
using NeoServer.Game.Systems.SafeTrade;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Creature;
using NeoServer.Server.Events.Items;
using NeoServer.Server.Events.Player;
using NeoServer.Server.Events.Player.Trade;
using NeoServer.Server.Events.Server;
using NeoServer.Server.Events.Tiles;

namespace NeoServer.Server.Events.Subscribers;

public sealed class EventSubscriber
{
    private readonly IServiceProvider _container;
    private readonly IGameServer _gameServer;

    private readonly IMap _map;
    private readonly SafeTradeSystem _tradeSystem;
    private readonly ItemStartedDecayingEventHandler _itemStartedDecayingEventHandler;
    private readonly ItemUsedOnTileEventHandler _itemUsedOnTileEventHandler;
    private readonly FieldRuneUsedEventHandler _fieldRuneUsedEventHandler;
    private readonly ItemUsedEventHandler _itemUsedEventHandler;
    public EventSubscriber(IMap map, IGameServer gameServer, IServiceProvider container, SafeTradeSystem tradeSystem, 
        ItemStartedDecayingEventHandler itemStartedDecayingEventHandler, ItemUsedOnTileEventHandler itemUsedOnTileEventHandler, FieldRuneUsedEventHandler fieldRuneUsedEventHandler, ItemUsedEventHandler itemUsedEventHandler)
    {
        _map = map;
        _gameServer = gameServer;
        _container = container;
        _tradeSystem = tradeSystem;
        _itemStartedDecayingEventHandler = itemStartedDecayingEventHandler;
        _itemUsedOnTileEventHandler = itemUsedOnTileEventHandler;
        _fieldRuneUsedEventHandler = fieldRuneUsedEventHandler;
        _itemUsedEventHandler = itemUsedEventHandler;
    }

    public void AttachEvents()
    {
        _map.OnCreatureAddedOnMap += (creature, cylinder) =>
            _container.GetRequiredService<CreatureAddedOnMapEventHandler>().Execute(creature, cylinder);

        _map.OnCreatureAddedOnMap += (creature, _) =>
            _container.GetRequiredService<PlayerSelfAppearOnMapEventHandler>().Execute(creature);

        _map.OnThingRemovedFromTile += _container.GetRequiredService<ThingRemovedFromTileEventHandler>().Execute;
        _map.OnCreatureMoved += _container.GetRequiredService<CreatureMovedEventHandler>().Execute;
        _map.OnThingMovedFailed += _container.GetRequiredService<InvalidOperationEventHandler>().Execute;
        _map.OnThingAddedToTile += _container.GetRequiredService<ThingAddedToTileEventHandler>().Execute;
        _map.OnThingUpdatedOnTile += _container.GetRequiredService<ThingUpdatedOnTileEventHandler>().Execute;

        BaseSpell.OnSpellInvoked += _container.GetRequiredService<SpellInvokedEventHandler>().Execute;

        OperationFailService.OnOperationFailed += _container.GetRequiredService<PlayerOperationFailedEventHandler>().Execute;
        OperationFailService.OnInvalidOperation += _container.GetRequiredService<PlayerOperationFailedEventHandler>().Execute;
        NotificationSenderService.OnNotificationSent += _container.GetRequiredService<NotificationSentEventHandler>().Execute;
        _gameServer.OnOpened += _container.GetRequiredService<ServerOpenedEventHandler>().Execute;
        
        IDecayable.OnStarted += _itemStartedDecayingEventHandler.Execute;
        IUsableOnTile.OnUsedOnTile += _itemUsedOnTileEventHandler.Execute;
        
        IConsumable.OnUsed += _itemUsedEventHandler.Execute;
        IUsableOnTile.OnUsedOnTile += _fieldRuneUsedEventHandler.Execute;
        
        AddTradeHandlers();
    }

    private void AddTradeHandlers()
    {
        _tradeSystem.OnClosed += _container.GetRequiredService<TradeClosedEventHandler>().Execute;
        _tradeSystem.OnTradeRequest += _container.GetRequiredService<TradeRequestedEventHandler>().Execute;
    }
}