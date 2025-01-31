using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Custom;

public class OpcodeMessagePacket : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ExtendedOpcode;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(0x00);
        message.AddUInt16(0x00);
    }
}