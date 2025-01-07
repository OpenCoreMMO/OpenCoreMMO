using NeoServer.Common.Network.Packets.Enums;
using NeoServer.Common.Network.Packets.Extensions;

namespace NeoServer.Common.Network.Packets.ClientToServer;

public readonly struct PlayerSayToPrivatePacket
{
    private readonly Memory<byte> _data;

    public PlayerSayToPrivatePacket()
    {
        _data = new Memory<byte>();
    }

    private PlayerSayToPrivatePacket(Memory<byte> data)
    {
        _data = data;
    }

    public static byte Code => 0x96;

    public SpeechType TalkType
    {
        get => (SpeechType)_data.Span[0];
        set => _data.Span[0] = (byte)value;
    }

    public string Receiver
    {
        get => _data.Span.ExtractString(1, 30, System.Text.Encoding.UTF8);
        set => _data.Slice(1, 30).Span.WriteString(value, System.Text.Encoding.UTF8);
    }

    public string Message
    {
        get=> _data.Span.ExtractString(31, 200, System.Text.Encoding.UTF8);
        set => _data.Slice(31, 200).Span.WriteString(value, System.Text.Encoding.UTF8);
    }

    public static implicit operator PlayerSayToPrivatePacket(Memory<byte> packet) => new(packet);

    public static implicit operator Memory<byte>(PlayerSayToPrivatePacket packet) => packet._data;
}
