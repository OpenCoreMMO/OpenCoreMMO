using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveItemContainerPacket(byte containerId, byte slotIndex) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ContainerRemoveItem;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(containerId);
        message.AddByte(slotIndex);
    }
}