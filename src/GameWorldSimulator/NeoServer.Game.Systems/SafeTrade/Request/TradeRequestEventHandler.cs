using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Systems.SafeTrade.Trackers;

namespace NeoServer.Game.Systems.SafeTrade.Request;

internal static class TradeRequestEventHandler
{
    private static Action<TradeRequest> CancelTradeAction { get; set; }
    private static HashSet<uint> PlayerEventSubscription { get; } = new();

    /// <summary>
    ///     Initializes the TradeRequestEventHandler with the cancelTradeAction.
    /// </summary>
    public static void Init(Action<TradeRequest> cancelTradeAction)
    {
        CancelTradeAction = cancelTradeAction;
    }

    /// <summary>
    ///     Subscribes to the events of the given player and item, if they exist.
    /// </summary>
    public static void Subscribe(IPlayer player, IItem[] items)
    {
        if (player is not null && !PlayerEventSubscription.Contains(player.CreatureId))
        {
            player.OnCreatureMoved += OnPlayerMoved;
            player.OnLoggedOut += OnPlayerLogout;
            player.OnDeath += OnPlayerDeath;

            // Add player ID to the HashSet to prevent multiple subscriptions
            PlayerEventSubscription.Add(player.CreatureId);
        }

        if (items is null) return;

        SubscribeToItems(items);
    }

    private static void SubscribeToItems(IItem[] items)
    {
        foreach (var item in items)
        {
            if (item is null) continue;

            item.OnDeleted += ItemDeleted;
            item.OnRemoved += ItemRemoved;

            switch (item)
            {
                case ICumulative cumulative:
                    cumulative.OnReduced += ItemReduced;
                    break;
                case IContainer container:
                    container.OnItemAdded += ItemAddedToContainer;
                    container.OnItemUpdated += OnItemUpdatedOnContainer;
                    break;
            }
        }
    }

    /// <summary>
    ///     Unsubscribes from the events of the given player and item, if they exist.
    /// </summary>
    public static void Unsubscribe(IPlayer player, IItem[] items)
    {
        if (player is not null)
        {
            player.OnCreatureMoved -= OnPlayerMoved;
            player.OnLoggedOut -= OnPlayerLogout;
            player.OnDeath -= OnPlayerDeath;

            // Remove player ID from the HashSet to allow future subscriptions
            PlayerEventSubscription.Remove(player.CreatureId);
        }

        if (items is null) return;

        UnsubscribeFromItems(items);
    }

    private static void UnsubscribeFromItems(IItem[] items)
    {
        foreach (var item in items)
        {
            if (item is null) continue;

            item.OnDeleted -= ItemDeleted;
            item.OnRemoved -= ItemRemoved;

            switch (item)
            {
                case ICumulative cumulative:
                    cumulative.OnReduced -= ItemReduced;
                    break;
                case IContainer container:
                    container.OnItemAdded -= ItemAddedToContainer;
                    container.OnItemUpdated -= OnItemUpdatedOnContainer;
                    break;
            }
        }
    }

    private static void ItemAddedToContainer(IItem item, IContainer container)
    {
        var tradeRequest = ItemTradedTracker.GetTradeRequest(container);
        if (tradeRequest is null) return;

        CancelTradeAction?.Invoke(tradeRequest);
    }

    private static void OnItemUpdatedOnContainer(IContainer onContainer, byte slotIndex, IItem item, sbyte amount)
    {
        var tradeRequest = ItemTradedTracker.GetTradeRequest(onContainer);
        if (tradeRequest is null) return;

        CancelTradeAction?.Invoke(tradeRequest);
    }

    private static void OnPlayerDeath(ICombatActor creature, IThing by)
    {
        if (creature is not IPlayer player) return;
        var tradeRequest = TradeRequestTracker.GetTradeRequest(player);

        if (tradeRequest is null) return;

        CancelTradeAction?.Invoke(tradeRequest);
    }

    //Cancel the trade if player moves from a location that is more than one SQM away from the other player
    private static void OnPlayerMoved(IWalkableCreature creature, Location fromLocation, Location toLocation,
        ICylinderSpectator[] spectators)
    {
        if (creature is not IPlayer player) return;

        var tradeRequest = TradeRequestTracker.GetTradeRequest(player);

        if (tradeRequest is null) return;

        var item = tradeRequest.Items[0];

        var itemLocation = item.Owner?.Location ?? item.Location;
        var isFarFromItem = item.Owner is not IPlayer &&
                            creature.Location.GetMaxSqmDistance(itemLocation) > 1;

        if (isFarFromItem)
        {
            CancelTradeAction?.Invoke(tradeRequest);
            return;
        }

        // Check if the player moved to a location that is not next to the other player
        var isFarFromSecondPlayer = creature.Location.GetMaxSqmDistance(tradeRequest.PlayerRequested.Location) > 2;
        if (isFarFromSecondPlayer) CancelTradeAction?.Invoke(tradeRequest);
    }

    private static void OnPlayerLogout(IPlayer player)
    {
        var tradeRequest = TradeRequestTracker.GetTradeRequest(player);

        if (tradeRequest is null) return;

        CancelTradeAction?.Invoke(tradeRequest);
    }

    private static void ItemRemoved(IItem item, IThing _)
    {
        CancelTrade(item);
    }

    private static void ItemDeleted(IItem item)
    {
        CancelTrade(item);
    }

    private static void ItemReduced(ICumulative item, byte amount)
    {
        CancelTrade(item);
    }

    private static void CancelTrade(IItem item)
    {
        var tradeRequest = ItemTradedTracker.GetTradeRequest(item);
        if (tradeRequest is null) return;

        CancelTradeAction?.Invoke(tradeRequest);
    }
}