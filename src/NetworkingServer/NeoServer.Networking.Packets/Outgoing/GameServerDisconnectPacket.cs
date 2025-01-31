using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing;

public sealed class GameServerDisconnectPacket(string reason) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.Disconnect;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddString(reason);
    }
}