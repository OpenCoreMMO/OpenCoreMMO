using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class PlayerInventoryPacket : OutgoingPacket
{
    private readonly IInventory inventory;
    public required bool ShowItemDescription { get; init; }

    public PlayerInventoryPacket(IInventory inventory)
    {
        this.inventory = inventory;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        var addInventoryItem = new Action<Slot>(slot =>
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
        });

        addInventoryItem(Slot.Head);
        addInventoryItem(Slot.Necklace);
        addInventoryItem(Slot.Backpack);
        addInventoryItem(Slot.Body);
        addInventoryItem(Slot.Right);
        addInventoryItem(Slot.Left);
        addInventoryItem(Slot.Legs);
        addInventoryItem(Slot.Feet);
        addInventoryItem(Slot.Ring);
        addInventoryItem(Slot.Ammo);
    }
}