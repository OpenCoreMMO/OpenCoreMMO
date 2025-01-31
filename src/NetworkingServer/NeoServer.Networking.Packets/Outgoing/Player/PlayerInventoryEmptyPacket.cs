using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerInventoryEmptyPacket : OutgoingPacket
{
    public required Slot Slot { get; init; }
    public override byte PacketType => (byte)GameOutgoingPacketType.InventoryEmpty;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddSlot(Slot);
    }
}