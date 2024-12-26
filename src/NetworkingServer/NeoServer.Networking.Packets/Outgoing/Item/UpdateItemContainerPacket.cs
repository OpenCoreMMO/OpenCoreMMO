using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class UpdateItemContainerPacket : OutgoingPacket
{
    private readonly byte containerId;
    private readonly IItem item;
    public readonly byte slot;

    public UpdateItemContainerPacket(byte containerId, byte slot, IItem item)
    {
        this.containerId = containerId;
        this.item = item;
        this.slot = slot;
    }

    public required bool ShowItemDescription { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ContainerUpdateItem);

        message.AddByte(containerId);
        message.AddByte(slot);
        message.AddItem(item, ShowItemDescription);
    }
}