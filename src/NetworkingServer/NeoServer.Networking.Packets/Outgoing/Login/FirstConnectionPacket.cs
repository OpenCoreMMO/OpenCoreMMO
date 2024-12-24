using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Login;

public class FirstConnectionPacket : OutgoingPacket
{
    public required long TimeStamp { get; set; }
    public required byte RandomNumber { get; set; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddUInt32(0);
        message.AddUInt16(0x0006);
        message.AddByte(0x1F);
        message.AddUInt32((uint)TimeStamp);
        message.AddByte(RandomNumber);
        message.WriteUint32(NeoServer.Server.Security.AdlerChecksum.Checksum(message.Buffer,4, message.Length - 4), 0);
    }
}