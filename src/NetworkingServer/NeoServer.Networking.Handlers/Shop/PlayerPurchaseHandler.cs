using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Networking.Packets.Incoming.Shop;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Shop;

public class PlayerPurchaseHandler : PacketHandler
{
    private readonly IGameCreatureManager _creatureManager;
    private readonly IDealTransaction _dealTransaction;
    private readonly IDispatcher _dispatcher;
    private readonly IItemTypeStore _itemTypeStore;

    public PlayerPurchaseHandler(IDealTransaction dealTransaction, IItemTypeStore itemTypeStore,
        IGameCreatureManager creatureManager, IDispatcher dispatcher)
    {
        _dealTransaction = dealTransaction;
        _itemTypeStore = itemTypeStore;
        _creatureManager = creatureManager;
        _dispatcher = dispatcher;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var playerPurchasePacket = new PlayerPurchasePacket(message);
        if (!_creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var serverId = ItemClientServerIdMapStore.Data.Get(playerPurchasePacket.ItemClientId);

        if (!_itemTypeStore.TryGetValue(serverId, out var itemType)) return;

        _dispatcher.AddEvent(new Event(() =>
            _dealTransaction?.Buy(player, player.TradingWithNpc, itemType, playerPurchasePacket.Amount)));
    }
}