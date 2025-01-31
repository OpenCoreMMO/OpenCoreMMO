using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Trade;

public class TradeRequestPacket : OutgoingPacket
{
    public TradeRequestPacket(string playerName, IItem[] items, bool acknowledged = false)
    {
        PlayerName = playerName;
        Items = items;
        Acknowledged = acknowledged;
    }

    private string PlayerName { get; }
    private IItem[] Items { get; }
    private bool Acknowledged { get; }
    public required bool ShowItemDescription { get; init; }

    //todo: muniz separate in two packets
    public override byte PacketType => (byte)GameOutgoingPacketType.AcknowlegdeTradeRequest;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(Acknowledged
            ? (byte)GameOutgoingPacketType.AcknowlegdeTradeRequest
            : (byte)GameOutgoingPacketType.TradeRequest);

        message.AddString(PlayerName);

        message.AddByte((byte)Items.Length);
        foreach (var item in Items) message.AddItem(item, ShowItemDescription);
    }
}