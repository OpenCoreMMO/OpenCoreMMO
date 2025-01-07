using NeoServer.Common.Network.Packets.Enums;
using NeoServer.Common.Network.Packets.Extensions;

namespace NeoServer.Common.Network.Packets.ClientToServer;

public readonly struct PlayerSayToChannelPacket
{
    private readonly Memory<byte> _data;

    public PlayerSayToChannelPacket()
    {
        _data = new Memory<byte>();
    }

    private PlayerSayToChannelPacket(Memory<byte> data)
    {
        _data = data;
    }

    public static byte Code => 0x96;

    public SpeechType TalkType
    {
        get => (SpeechType)_data.Span[0];
        set => _data.Span[0] = (byte)value;
    }

    public ushort ChannelId
    {
        get => BitConverter.ToUInt16(_data.Span.Slice(1, 2));
        set => BitConverter.TryWriteBytes(_data.Span.Slice(1, 2), value);
    }

    public string Message
    {
        get => _data.Span.ExtractString(3, 200, System.Text.Encoding.UTF8);
        set => _data.Slice(3, 200).Span.WriteString(value, System.Text.Encoding.UTF8);
    }

    public static implicit operator PlayerSayToChannelPacket(Memory<byte> packet) => new(packet);

    public static implicit operator Memory<byte>(PlayerSayToChannelPacket packet) => packet._data;
}
