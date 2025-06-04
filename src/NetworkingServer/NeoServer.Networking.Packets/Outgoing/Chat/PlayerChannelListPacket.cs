using NeoServer.Domain.Chat;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerChannelListPacket : OutgoingPacket
{
    private readonly ChatChannel[] chatChannels;

    public PlayerChannelListPacket(ChatChannel[] chatChannels)
    {
        this.chatChannels = chatChannels;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.ChannelList);

        message.AddByte((byte)chatChannels.Length);

        foreach (var channel in chatChannels)
        {
            message.AddUInt16(channel.Id);
            message.AddString(channel.Name);
        }
    }
}