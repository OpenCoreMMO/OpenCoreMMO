using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureMovedUpPacket(byte[] floorsDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.FloorChangeUp;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(floorsDescription);
    }
}