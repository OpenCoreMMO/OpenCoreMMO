using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Outgoing.Npc;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature.Npcs;

public class NpcShowShopEventHandler
{
    private readonly ICoinTypeStore _coinTypeStore;
    private readonly IGameServer _game;

    public NpcShowShopEventHandler(IGameServer game, ICoinTypeStore coinTypeStore)
    {
        _game = game;
        _coinTypeStore = coinTypeStore;
    }

    public void Execute(INpc npc, ISociableCreature to, IEnumerable<IShopItem> shopItems)
    {
        if (!_game.CreatureManager.GetPlayerConnection(to.CreatureId, out var connection)) return;

        shopItems = shopItems.ToList();

        connection.OutgoingPackets.Enqueue(new OpenShopPacket(shopItems));

        if (to is IPlayer { Shopping: true } player)
            connection.OutgoingPackets.Enqueue(new SaleItemListPacket(player, shopItems, _coinTypeStore));
        connection.Send();
    }
}