using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Texts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Effect;

public class AnimatedTextPacket : OutgoingPacket
{
    private readonly TextColor color;
    private readonly Location location;
    private readonly string text;

    public AnimatedTextPacket(Location location, TextColor color, string message)
    {
        this.location = location;
        this.color = color;
        text = message;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.AnimatedText);
        message.AddLocation(location);
        message.AddByte((byte)color);
        message.AddString(text);
    }
}