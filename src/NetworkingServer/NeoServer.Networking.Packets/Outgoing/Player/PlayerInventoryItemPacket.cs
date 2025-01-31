using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerInventoryItemPacket() : OutgoingPacket
{
    public required Slot Slot { get; init; }
    public required ushort ItemClientId { get; init; }
    public required string ItemDescription { get; init; }

    public override byte PacketType => (byte)GameOutgoingPacketType.InventoryItem;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte((byte)Slot);
        message.AddUInt16(ItemClientId);

        if(!string.IsNullOrEmpty(ItemDescription))
            message.AddString(ItemDescription);
    }
}