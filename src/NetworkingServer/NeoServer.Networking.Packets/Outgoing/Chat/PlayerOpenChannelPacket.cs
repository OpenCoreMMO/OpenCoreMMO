using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerOpenChannelPacket(ushort channelId, string name) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.OpenChannel;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16(channelId);
        message.AddString(name);
    }
}