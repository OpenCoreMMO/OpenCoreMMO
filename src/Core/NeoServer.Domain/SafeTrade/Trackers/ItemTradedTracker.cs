using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.SafeTrade.Request;

namespace NeoServer.Domain.SafeTrade.Trackers;

/// <summary>
///     A static class for tracking the trade requests associated with items.
/// </summary>
internal static class ItemTradedTracker
{
    private static readonly Dictionary<IItem, TradeRequest> ItemToTradeRequestMap = new();

    public static void TrackItems(IItem[] items, TradeRequest tradeRequest)
    {
        if (items is null) return;

        foreach (var item in items)
        {
            if (item is null) continue;

            ItemToTradeRequestMap[item] = tradeRequest;
        }
    }

    public static bool UntrackItems(IEnumerable<IItem> items)
    {
        if (items is null) return false;

        foreach (var item in items)
        {
            if (item is null) continue;

            ItemToTradeRequestMap.Remove(item);
        }

        return true;
    }

    public static TradeRequest GetTradeRequest(IItem item)
    {
        return ItemToTradeRequestMap.TryGetValue(item, out var tradeRequest) ? tradeRequest : null;
    }
}