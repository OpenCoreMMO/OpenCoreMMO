using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class CancelTargetPacket : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.CancelTarget;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(0x00);
    }
}