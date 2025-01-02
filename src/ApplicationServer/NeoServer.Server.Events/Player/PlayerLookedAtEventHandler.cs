using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerLookedAtEventHandler(IGameServer game)
{
    public void Execute(IPlayer player, IThing thing, bool isClose)
    {
        if (game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection) is false) return;


        var text = thing.GetLookText(isClose, player.CanSeeInspectionDetails);

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(text, TextMessageOutgoingType.Description));
        connection.Send();
    }
}