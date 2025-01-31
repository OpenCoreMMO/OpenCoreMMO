using System;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Player;

public class OpenContainerPacket(IContainer container, byte containerId, bool withDescription) : OutgoingPacket
{
    public override byte PacketType => (byte)GameOutgoingPacketType.ContainerOpen;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte(PacketType);
        message.AddByte(containerId);
        message.AddItem(container, withDescription);
        message.AddString(container.Name);
        message.AddByte(container.Capacity);

        message.AddByte(container.HasParent ? (byte)0x01 : (byte)0x00);

        message.AddByte(Math.Min((byte)0xFF, container.SlotsUsed));

        for (byte i = 0; i < container.SlotsUsed; i++) message.AddItem(container.Items[i], withDescription);
    }
}