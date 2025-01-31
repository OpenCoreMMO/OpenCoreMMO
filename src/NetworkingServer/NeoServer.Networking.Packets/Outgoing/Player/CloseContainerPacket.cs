using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class CloseContainerPacket(byte containerId) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ContainerClose;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(containerId);
    }
}