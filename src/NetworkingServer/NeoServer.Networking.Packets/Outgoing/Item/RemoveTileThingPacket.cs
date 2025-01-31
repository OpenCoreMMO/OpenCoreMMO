using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class RemoveTileThingPacket(Location location, byte stackPosition) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.RemoveAtStackPos;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddByte(stackPosition);
    }
}