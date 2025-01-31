using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureMovedDownPacket(byte[] floorsDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.FloorChangeDown;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddBytes(floorsDescription);
    }
}