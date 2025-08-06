using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureChangeSpeedPacket(uint creaturedId, ushort speed, uint baseSpeed) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ChangeSpeed);

        message.AddUInt32(creaturedId);
        message.AddUInt16((ushort)(baseSpeed / 2));
        message.AddUInt16((ushort)(speed / 2));
    }
}