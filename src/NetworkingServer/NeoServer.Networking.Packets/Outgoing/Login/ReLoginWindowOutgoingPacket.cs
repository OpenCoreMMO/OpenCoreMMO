using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class ReLoginWindowOutgoingPacket : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ReLoginWindow;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
    }
}