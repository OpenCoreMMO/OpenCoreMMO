using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Chat;

public class PlayerAddOrRemoveVipPacket(uint playerId, string playerName, bool status) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.AddOrRemoveVip;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddUInt32(playerId);
        message.AddString(playerName);
        message.AddByte(status ? (byte)1 : (byte)0);
    }
}