using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Window;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerReadTextEventHandler : IEventHandler
{
    private readonly IGameServer game;

    public PlayerReadTextEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, IReadable readable, string text)
    {
        if (Guard.AnyNull(player, readable)) return;

        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        var id = ItemTextWindowStore.Add(player, readable);

        connection.OutgoingPackets.Enqueue(new TextWindowPacket(id, readable));

        connection.Send();
    }
}