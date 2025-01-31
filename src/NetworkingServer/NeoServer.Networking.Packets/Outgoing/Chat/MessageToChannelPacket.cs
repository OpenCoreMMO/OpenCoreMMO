using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class MessageToChannelPacket(ICreature from, SpeechType speechType, string textMessage, ushort channelId) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.SendPrivateMessage;

    public override void WriteToMessage(INetworkMessage message)
    {
        if (speechType == SpeechType.None) return;
        if (string.IsNullOrWhiteSpace(textMessage)) return;
        if (channelId == default) return;

        message.AddByte(PacketType);
        message.AddUInt32(0x00);

        if (speechType == SpeechType.ChannelRed2Text)
        {
            message.AddString(string.Empty);
            speechType = SpeechType.ChannelRed1Text;
        }
        else
        {
            if (from is not null)
                message.AddString(from.Name);
            else
                message.AddString(string.Empty);
            //Add level only for players
            if (from is IPlayer player)
                message.AddUInt16(player.Level);
            else
                message.AddUInt16(0x00);
        }

        message.AddByte((byte)speechType);
        message.AddUInt16(channelId);
        message.AddString(textMessage);
    }
}