using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Npc;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;

namespace NeoServer.Server.Events.Player;

public class PlayerChangedInventoryEventHandler
{
    private readonly ClientConfiguration _clientConfiguration;
    private readonly ICoinTypeStore _coinTypeStore;
    private readonly IGameServer game;

    public PlayerChangedInventoryEventHandler(IGameServer game, ICoinTypeStore coinTypeStore,
        ClientConfiguration clientConfiguration)
    {
        this.game = game;
        _coinTypeStore = coinTypeStore;
        _clientConfiguration = clientConfiguration;
    }

    public void Execute(IInventory inventory, IItem item, Slot slot, byte amount = 1)
    {
        if (Guard.AnyNull(inventory)) return;

        var player = inventory.Owner;

        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerInventoryItemPacket(player.Inventory, slot)
        {
            ShowItemDescription = connection.OtcV8Version > 0 && _clientConfiguration.OtcV8.GameItemTooltip
        });

        if (player.Shopping)
            connection.OutgoingPackets.Enqueue(new SaleItemListPacket(player,
                player.TradingWithNpc?.ShopItems?.Values, _coinTypeStore));

        connection.Send();
    }

    public void ExecuteOnWeightChanged(IInventory inventory)
    {
        if (Guard.AnyNull(inventory)) return;

        var player = inventory.Owner;

        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(player));

        connection.Send();
    }
}