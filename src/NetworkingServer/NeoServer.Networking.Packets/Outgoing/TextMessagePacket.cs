using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing;

public class TextMessagePacket : OutgoingPacket
{
    private readonly string text;
    private readonly TextMessageOutgoingType type;

    public TextMessagePacket(string text, TextMessageOutgoingType type)
    {
        this.text = text;
        this.type = type;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.TextMessage);
        message.AddByte((byte)type);

        //todo: 1098 impelment this
        switch (type)
        {
            case TextMessageOutgoingType.MESSAGE_DAMAGE_DEALT:
            case TextMessageOutgoingType.MESSAGE_DAMAGE_RECEIVED:
            case TextMessageOutgoingType.MESSAGE_DAMAGE_OTHERS:
                //message.AddLocation(Data.Position);
                //message.AddUInt32(Data.Primary.Value);
                //message.AddByte(Data.Primary.Color);
                //message.AddUInt32(Data.Secondary.Value);
                //message.AddByte(Data.Secondary.Color);
                break;

            case TextMessageOutgoingType.MESSAGE_HEALED:
            case TextMessageOutgoingType.MESSAGE_HEALED_OTHERS:
            case TextMessageOutgoingType.MESSAGE_EXPERIENCE:
            case TextMessageOutgoingType.MESSAGE_EXPERIENCE_OTHERS:
                //message.AddLocation(Data.Position);
                //message.AddUInt32(Data.Primary.Value);
                //message.AddByte(Data.Primary.Color);
                break;

            case TextMessageOutgoingType.MESSAGE_GUILD:
            case TextMessageOutgoingType.MESSAGE_PARTY_MANAGEMENT:
            case TextMessageOutgoingType.MESSAGE_PARTY:
                //message.AddUInt16(Data.ChannelId);
                break;

            default:
                break;
        }

        message.AddString(text);
    }
}