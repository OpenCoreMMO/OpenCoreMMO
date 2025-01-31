using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerOpenPrivateChannelPacket(string receiver) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.OpenPrivateChannel;

    //todo: this code is duplicated?
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddString(receiver);
    }
}