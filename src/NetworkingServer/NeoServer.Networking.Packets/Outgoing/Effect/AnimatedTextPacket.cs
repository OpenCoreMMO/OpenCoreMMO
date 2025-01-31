using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Texts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Effect;

public class AnimatedTextPacket(Location location, TextColor color, string message) : OutgoingPacket
{
    private readonly string text = message;

    public override byte PacketType => (byte)GameOutgoingPacketType.AnimatedText;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddLocation(location);
        message.AddByte((byte)color);
        message.AddString(text);
    }
}