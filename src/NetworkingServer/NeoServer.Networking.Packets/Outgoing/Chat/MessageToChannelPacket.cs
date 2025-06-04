using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class MessageToChannelPacket : OutgoingPacket
{
    public MessageToChannelPacket(ICreature from, SpeechType talkType, string message, ushort channelId)
    {
        From = from;
        TalkType = talkType;
        Message = message;
        ChannelId = channelId;
    }

    public ICreature From { get; }
    public SpeechType TalkType { get; }
    public string Message { get; }
    public ushort ChannelId { get; }

    public override void WriteToMessage(INetworkMessage message)
    {
        if (TalkType == SpeechType.None) return;
        if (string.IsNullOrWhiteSpace(Message)) return;
        if (ChannelId == default) return;

        message.AddByte((byte)GameOutgoingPacketType.SendPrivateMessage);
        message.AddUInt32(0x00);

        var speechType = TalkType;

        if (speechType == SpeechType.ChannelRed2Text)
        {
            message.AddString(string.Empty);
            speechType = SpeechType.ChannelRed1Text;
        }
        else
        {
            if (From is not null)
                message.AddString(From.Name);
            else
                message.AddString(string.Empty);
            //Add level only for players
            if (From is IPlayer player)
                message.AddUInt16(player.Level);
            else
                message.AddUInt16(0x00);
        }

        message.AddByte((byte)speechType);
        message.AddUInt16(ChannelId);
        message.AddString(Message);
    }
}