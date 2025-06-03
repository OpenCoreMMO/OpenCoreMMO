using NeoServer.Domain.Common.Chats;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class PlayerSayPacket : IncomingPacket
{
    public PlayerSayPacket(IReadOnlyNetworkMessage message)
    {
        TalkType = (SpeechType)message.GetByte();

        switch (TalkType)
        {
            case SpeechType.None:
                return;

            case SpeechType.Private:
            case SpeechType.PrivateRed:
#if GAME_FEATURE_RULEVIOLATION
		        case TALKTYPE_RVR_ANSWER:
#endif
                Receiver = message.GetString();
                break;

            case SpeechType.ChannelYellowText:
            case SpeechType.ChannelRed1Text:
            case SpeechType.ChannelOrangeText:
                ChannelId = message.GetUInt16();
                break;
            default:
                ChannelId = ushort.MinValue;
                break;
        }

        Message = message.GetString();
    }

    public virtual SpeechType TalkType { get; }
    public virtual string Receiver { get; set; }
    public virtual string Message { get; }
    public virtual ushort ChannelId { get; set; }
}