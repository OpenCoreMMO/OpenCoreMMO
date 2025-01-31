using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Effect;

public class DistanceEffectPacket(Location location, Location toLocation, byte effect) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.DistanceShootEffect;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddLocation(toLocation);
        message.AddByte(effect);
    }
}