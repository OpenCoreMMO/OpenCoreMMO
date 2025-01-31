using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerCloseChannelPacket(ushort channelId) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CloseChannel;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16(channelId);
    }
}