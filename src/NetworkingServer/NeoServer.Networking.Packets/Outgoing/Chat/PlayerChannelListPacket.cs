using NeoServer.Game.Common.Contracts.Chats;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerChannelListPacket(IChatChannel[] chatChannels) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ChannelList;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte((byte)chatChannels.Length);

        foreach (var channel in chatChannels)
        {
            message.AddUInt16(channel.Id);
            message.AddString(channel.Name);
        }
    }
}