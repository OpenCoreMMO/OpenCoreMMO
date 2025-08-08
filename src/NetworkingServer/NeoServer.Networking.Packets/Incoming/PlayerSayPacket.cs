using NeoServer.Domain.Chat;
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

            case SpeechType.PrivateRedTo:
            case SpeechType.PrivateRedFrom:
#if GAME_FEATURE_RULEVIOLATION
		        case TALKTYPE_RVR_ANSWER:
#endif
                Receiver = message.GetString();
                break;

            case SpeechType.ChannelYellow:
            case SpeechType.ChannelRed1:
            case SpeechType.ChannelOrange:
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