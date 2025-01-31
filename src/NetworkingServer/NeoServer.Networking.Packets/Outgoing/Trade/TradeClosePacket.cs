using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Trade;

public class TradeClosePacket : OutgoingPacket
{
    public TradeClosePacket() { }

    public TradeClosePacket(IReadOnlyNetworkMessage message) { }

    public override byte PacketType => (byte)GameOutgoingPacketType.TradeClose;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
    }
}