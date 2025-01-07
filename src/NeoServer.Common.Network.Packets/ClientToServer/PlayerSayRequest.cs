using NeoServer.Common.Network.Packets.Enums;
using NeoServer.Common.Network.Packets.Extensions;

namespace NeoServer.Common.Network.Packets.ClientToServer;

public readonly struct PlayerSayPacket
{
    private readonly Memory<byte> _data;

    private PlayerSayPacket(Memory<byte> data)
    {
        _data = data;
    }

    public static byte Code => 0x96;

    public SpeechType TalkType
    {
        get => (SpeechType)_data.Span[0]; // Primeiro byte do pacote
        set => _data.Span[0] = (byte)value;
    }

    public string Receiver
    {
        get
        {
            switch (TalkType)
            {
                case SpeechType.Private:
                case SpeechType.PrivateRed:
#if GAME_FEATURE_RULEVIOLATION
                case SpeechType.RvrAnswer:
#endif
                    return _data.Span.ExtractString(1, 30, System.Text.Encoding.UTF8);
                default:
                    return string.Empty;
            }
        }
        set
        {
            switch (TalkType)
            {
                case SpeechType.Private:
                case SpeechType.PrivateRed:
#if GAME_FEATURE_RULEVIOLATION
                case SpeechType.RvrAnswer:
#endif
                    _data.Slice(1, 30).Span.WriteString(value, System.Text.Encoding.UTF8);
                    break;
            }
        }
    }

    public ushort ChannelId
    {
        get
        {
            switch (TalkType)
            {
                case SpeechType.ChannelYellowText:
                case SpeechType.ChannelRed1Text:
                case SpeechType.ChannelOrangeText:
                    return BitConverter.ToUInt16(_data.Span.Slice(1, 2));
                default:
                    return ushort.MinValue;
            }
        }
        set
        {
            switch (TalkType)
            {
                case SpeechType.ChannelYellowText:
                case SpeechType.ChannelRed1Text:
                case SpeechType.ChannelOrangeText:
                    BitConverter.TryWriteBytes(_data.Span.Slice(1, 2), value);
                    break;
            }
        }
    }

    public string Message
    {
        get
        {
            int offset = 1;
            if (TalkType == SpeechType.Private || TalkType == SpeechType.PrivateRed)
                offset = 31;
            else if (TalkType == SpeechType.ChannelYellowText || TalkType == SpeechType.ChannelRed1Text || TalkType == SpeechType.ChannelOrangeText)
                offset = 3;

            return _data.Span.ExtractString(offset, 200, System.Text.Encoding.UTF8);
        }
        set
        {
            int offset = 1;
            if (TalkType == SpeechType.Private || TalkType == SpeechType.PrivateRed)
                offset = 31;
            else if (TalkType == SpeechType.ChannelYellowText || TalkType == SpeechType.ChannelRed1Text || TalkType == SpeechType.ChannelOrangeText)
                offset = 3;

            _data.Slice(offset, 200).Span.WriteString(value, System.Text.Encoding.UTF8);
        }
    }

    public static implicit operator PlayerSayPacket(Memory<byte> packet) => new(packet);

    public static implicit operator Memory<byte>(PlayerSayPacket packet) => packet._data;
}
