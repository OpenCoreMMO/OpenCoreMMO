using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Player.House;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerHouseWindowHandler(
    IGameServer game,
    PlayerEditHouseAccessListCommand editHouseAccessListCommand,
    IHouseEditWindowStore houseEditWindowStore) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var packet = new UpdateHouseWindowPacket(message);

        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
        {
            return;
        }

        // Packet door byte must be 0; the edited list comes from setEditHouse state.
        if (packet.DoorId != 0)
        {
            houseEditWindowStore.Clear(player);
            return;
        }

        if (!houseEditWindowStore.TryGet(player, packet.WindowTextId, out var houseId, out var listId))
        {
            return;
        }

        var text = packet.Text ?? string.Empty;

        game.Dispatcher.AddEvent(new Event(() =>
        {
            editHouseAccessListCommand.Execute(player, houseId, listId, text);
            houseEditWindowStore.Clear(player);
        }));
    }
}
