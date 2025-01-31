using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddTileItemPacket(IItem item, byte stackPosition) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.AddAtStackPos;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(item.Location);
        message.AddByte(stackPosition);
        message.AddItem(item);
    }
}