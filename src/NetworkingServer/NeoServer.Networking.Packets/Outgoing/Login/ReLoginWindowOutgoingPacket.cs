using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class ReLoginWindowOutgoingPacket : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ReLoginWindow);
        message.AddByte(0x00);
        message.AddByte((byte)100); //todo: 1098 implement this unfairFightReduction
    }
}