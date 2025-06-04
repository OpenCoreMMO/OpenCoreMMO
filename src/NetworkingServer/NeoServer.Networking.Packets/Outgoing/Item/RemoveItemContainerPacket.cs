using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveItemContainerPacket : OutgoingPacket
{
    private readonly byte containerId;
    private readonly IItem item;
    private readonly byte slotIndex;

    public RemoveItemContainerPacket(byte containerId, byte slotIndex, IItem item)
    {
        this.containerId = containerId;
        this.slotIndex = slotIndex;
        this.item = item;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerRemoveItem);

        message.AddByte(containerId);

        message.AddByte(slotIndex);
    }
}