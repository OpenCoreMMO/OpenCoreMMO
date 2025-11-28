using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Player.Party;

public class PartyEnableSharedExperienceHandler(IGameServer game) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var experienceSharingActive = message.GetByte() == 1;
        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;
        if (player == null || !player.PlayerParty.IsInParty) return;
        player.PlayerParty.Party.IsSharedExperienceEnabled = experienceSharingActive;
        connection.Send(new TextMessagePacket(
            $"Party experience sharing is now {(experienceSharingActive ? "enabled" : "disabled")}.",
            TextMessageOutgoingType.Small));
    }
}