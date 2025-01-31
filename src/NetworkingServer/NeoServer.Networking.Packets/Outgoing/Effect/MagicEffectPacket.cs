using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Effect;

public class MagicEffectPacket(Location location, EffectT effect) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.MagicEffect;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddByte((byte)effect);
    }
}