using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerWalkCancelPacket : OutgoingPacket
{
    private readonly IPlayer player;

    public PlayerWalkCancelPacket(IPlayer player)
    {
        this.player = player;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.PlayerWalkCancel);
        message.AddByte((byte)player.Direction);
    }
}