using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureChangeSpeedPacket(uint creaturedId, ushort speed) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ChangeSpeed;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(creaturedId);
        message.AddUInt16(speed);
    }
}