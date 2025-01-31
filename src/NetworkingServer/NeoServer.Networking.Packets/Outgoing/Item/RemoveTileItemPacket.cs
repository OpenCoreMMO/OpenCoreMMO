using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveTileItemPacket(Location location, byte stackPosition, IItem item) : OutgoingPacket
{
    //todo: muniz check this, same packet of AddTileItemPacket?
    public override byte PacketType => (byte)GameOutgoingPacketType.AddAtStackPos;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddByte(stackPosition);
        message.AddItem(item);
    }
}