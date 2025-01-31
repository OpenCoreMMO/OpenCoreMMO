using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class ConditionIconPacket(ushort icons) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.PlayerConditions;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt16(icons);
    }
}