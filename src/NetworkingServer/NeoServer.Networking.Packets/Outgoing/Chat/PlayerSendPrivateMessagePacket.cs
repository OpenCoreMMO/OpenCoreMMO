using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerSendPrivateMessagePacket(ISociableCreature from, SpeechType talkType, string textMessage) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.SendPrivateMessage;

    public override void WriteToMessage(INetworkMessage message)
    {
        if (talkType == SpeechType.None) return;

        uint statementId = 0;

        message.AddByte(PacketType);
        message.AddUInt32(++statementId);

        if (from is not null)
        {
            message.AddString(from.Name);
            if (from is IPlayer player) message.AddUInt16(player.Level);
            else message.AddUInt16(0x00);
        }
        else
        {
            message.AddUInt16(0x00);
        }

        message.AddByte((byte)talkType);
        message.AddString(textMessage);
    }
}