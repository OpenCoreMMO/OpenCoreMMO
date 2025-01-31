using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Window;

public class TextWindowPacket : OutgoingPacket
{
    private readonly IReadable item;
    private readonly uint windowTextId;

    public TextWindowPacket(uint windowTextId, IReadable item)
    {
        this.windowTextId = windowTextId;
        this.item = item;
    }

    public override byte PacketType => (byte)GameOutgoingPacketType.ChangeSpeed;

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.TextWindow);
        message.AddUInt32(windowTextId);
        message.AddItem(item);

        AddTextLength(message);

        message.AddString(item.Text ?? string.Empty);

        AddWriterName(message);

        AddWrittenDate(message);
    }

    private void AddTextLength(INetworkMessage message)
    {
        if (item.CanWrite)
        {
            message.AddUInt16(item.MaxLength);
            return;
        }

        message.AddUInt16((ushort)(item.Text?.Length ?? 0x00));
    }

    private void AddWrittenDate(INetworkMessage message)
    {
        if (!item.WrittenOn.HasValue)
        {
            message.AddUInt16(0x00);
            return;
        }

        message.AddString(item.WrittenOn.Value.ToShortDateString());
    }

    private void AddWriterName(INetworkMessage message)
    {
        if (string.IsNullOrWhiteSpace(item.WrittenBy))
        {
            message.AddUInt16(0x00);
            return;
        }

        message.AddString(item.WrittenBy);
    }
}