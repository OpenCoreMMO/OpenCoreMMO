using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerSendPrivateMessagePacket : OutgoingPacket
{
    public PlayerSendPrivateMessagePacket(ISociableCreature from, SpeechType talkType, string message)
    {
        From = from;
        TalkType = talkType;
        Message = message;
    }

    public ISociableCreature From { get; }
    public SpeechType TalkType { get; }
    public string Message { get; }

    public override void WriteToMessage(INetworkMessage message)
    {
        if (TalkType == SpeechType.None) return;

        message.AddByte((byte)GameOutgoingPacketType.SendPrivateMessage);
        uint statementId = 0;

        message.AddUInt32(++statementId);

        if (From is not null)
        {
            message.AddString(From.Name);
            if (From is IPlayer player) message.AddUInt16(player.Level);
            else message.AddUInt16(0x00);
        }
        else
        {
            message.AddUInt16(0x00);
        }

        message.AddByte((byte)TalkType);
        message.AddString(Message);
    }
}