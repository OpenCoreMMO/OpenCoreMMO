using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerInventoryPacket : OutgoingPacket
{
    private readonly IInventory inventory;

    public PlayerInventoryPacket(IInventory inventory)
    {
        this.inventory = inventory;
    }

    public required bool ShowItemDescription { get; init; }

    public override void WriteToMessage(INetworkMessage message)
    {
        void SendInventoryItem(Slot slot)
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

        ;

        SendInventoryItem(Slot.Head);
        SendInventoryItem(Slot.Necklace);
        SendInventoryItem(Slot.Backpack);
        SendInventoryItem(Slot.Body);
        SendInventoryItem(Slot.Right);
        SendInventoryItem(Slot.Left);
        SendInventoryItem(Slot.Legs);
        SendInventoryItem(Slot.Feet);
        SendInventoryItem(Slot.Ring);
        SendInventoryItem(Slot.Ammo);
    }
}