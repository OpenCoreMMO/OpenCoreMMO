using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class UpdateItemContainerPacket(byte containerId, byte slot, IItem item, bool showItemDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ContainerUpdateItem;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(containerId);
        message.AddByte(slot);
        message.AddItem(item, showItemDescription);
    }
}