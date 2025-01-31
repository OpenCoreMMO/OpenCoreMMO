using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class WaitingInLinePacket(string textMessage, byte retryTime) : OutgoingPacket
{
    public override byte PacketType => (byte)LoginOutgoingPacketType.Waiting;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddString(textMessage);
        message.AddByte(retryTime);
    }
}