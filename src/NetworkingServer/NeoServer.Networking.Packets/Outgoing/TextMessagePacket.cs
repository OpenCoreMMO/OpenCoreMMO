using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing;

//Example of packet with Read and Write, to be add the same pattern in all others packets in the future.
public class TextMessagePacket : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.TextMessage;

    public string Text { get; }
    public TextMessageOutgoingType Type { get; }

    public TextMessagePacket(string text, TextMessageOutgoingType type)
    {
        Text = text;
        Type = type;
    }

    public TextMessagePacket(IReadOnlyNetworkMessage message) 
    {
        Text = message.GetString();
        Type = (TextMessageOutgoingType)message.GetByte();
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte((byte)Type);
        message.AddString(Text);
    }
}