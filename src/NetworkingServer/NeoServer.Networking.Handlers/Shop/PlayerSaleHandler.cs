using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Networking.Packets.Incoming.Shop;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Shop;

public class PlayerSaleHandler(
    IDealTransaction dealTransaction,
    IItemTypeStore itemTypeStore,
    IGameCreatureManager creatureManager,
    IItemClientServerIdMapStore itemClientServerIdMapStore,
    IDispatcher dispatcher) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var playerSalePacket = new PlayerSalePacket(message);
        if (!creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var serverId = itemClientServerIdMapStore.Get(playerSalePacket.ItemClientId);

        if (!itemTypeStore.TryGetValue(serverId, out var itemType)) return;

        dispatcher.AddEvent(new Event(() =>
            dealTransaction?.PlayerSellItem(player, player.TradingWithNpc, itemType, playerSalePacket.Amount, playerSalePacket.IgnoreEquipped)));
    }
}