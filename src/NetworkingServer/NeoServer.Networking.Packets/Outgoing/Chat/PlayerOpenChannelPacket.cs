using NeoServer.Server.Common.Contracts.Network;
using System;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerOpenChannelPacket : OutgoingPacket
{
    private readonly ushort channelId;
    private readonly string name;

    public PlayerOpenChannelPacket(ushort channelId, string name)
    {
        this.channelId = channelId;
        this.name = name;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.OpenChannel);
        message.AddUInt16(channelId);
        message.AddString(name);

        //todo: 1098 implement this
        //if(channelUsers) {
        //    msg.add<uint16_t>(channelUsers->size());
        //    for (const auto&it : *channelUsers) {
        //        msg.addString(it.second->getName());
        //    }
        //} else
        //{
        message.AddUInt16(0x00);
        //}

        //todo: 1098 implement this
        //if (invitedUsers)
        //{
        //    msg.add<uint16_t>(invitedUsers->size());
        //    for (const auto&it : *invitedUsers) {
        //        msg.addString(it.second->getName());
        //    }
        //}
        //else
        //{
        message.AddUInt16(0x00);
        //}
    }
}