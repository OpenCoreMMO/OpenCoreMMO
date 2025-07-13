using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Bases;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Networking.Packets.Outgoing.Item;
using Xunit;

namespace NeoServer.Networking.Tests.Packets.Outgoing;

public class RemoveTileItemPacketTest
{
    private Item CreateRegularItem(ushort id)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetName("item");

        return new Item(type, new Location(100, 100, 7));
    }

    [Fact]
    public void Constructor_Item_Null_Returns()
    {
        var sut = new RemoveTileItemPacket(new Location(100, 100, 7), 1, null);
        Assert.Equal(default, sut.location);
        Assert.Equal(default, sut.stackPosition);
    }

    [Fact]
    public void WriteToMessage_Adds_Bytes_To_NetworkMessage()
    {
        var sut = new RemoveTileItemPacket(new Location(100, 100, 7), 1, CreateRegularItem(20));
        var message = new NetworkMessage();
        sut.WriteToMessage(message);

        var expected = new byte[] { 0x6A, 100, 0, 100, 0, 7, 1, 20, 0 };

        Assert.Equal(expected, message.GetMessageInBytes().ToArray());
    }
}