using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Security;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class FirstConnectionPacket(uint timeStamp, byte randomNumber) : OutgoingPacket
{
    public override byte PacketType => (byte)LoginOutgoingPacketType.NoType;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddUInt32(0);
        message.AddUInt16(0x0006);
        message.AddByte(0x1F);
        message.AddUInt32(timeStamp);
        message.AddByte(randomNumber);
        message.WriteUint32(AdlerChecksum.Checksum(message.Buffer, 4, message.Length - 4), 0);
    }
}