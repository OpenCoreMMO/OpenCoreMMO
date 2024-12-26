using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class OpcodeMessagePacket : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(0x32);
        message.AddByte(0x00);
        message.AddUInt16(0x00);
    }
}