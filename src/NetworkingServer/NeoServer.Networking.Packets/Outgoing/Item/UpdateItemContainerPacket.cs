using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class UpdateItemContainerPacket(byte containerId, ushort slot, IItem item) : OutgoingPacket
{
    public readonly ushort slot = slot;

    public required bool ShowItemDescription { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerUpdateItem);

        message.AddByte(containerId);
        message.AddUInt16(slot);
        message.AddItem(item, ShowItemDescription);
    }
}