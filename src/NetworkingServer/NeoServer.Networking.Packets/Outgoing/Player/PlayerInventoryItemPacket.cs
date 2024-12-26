using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerInventoryItemPacket : OutgoingPacket
{
    private readonly IInventory inventory;
    private readonly Slot slot;
    public required bool ShowItemDescription { get; init; }

    public PlayerInventoryItemPacket(IInventory inventory, Slot slot)
    {
        this.inventory = inventory;
        this.slot = slot;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        if (inventory[slot] == null)
        {
            message.AddByte((byte)GameOutgoingPacketType.InventoryEmpty);
            message.AddByte((byte)slot);
        }
        else
        {
            message.AddByte((byte)GameOutgoingPacketType.InventoryItem);
            message.AddByte((byte)slot);
            message.AddItem(inventory[slot], ShowItemDescription);
        }
    }
}