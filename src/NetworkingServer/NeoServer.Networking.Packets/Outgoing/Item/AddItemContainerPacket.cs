using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddItemContainerPacket(byte containerId, IItem item, bool showItemDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ContainerAddItem;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(containerId);
        message.AddItem(item, showItemDescription);
    }
}