using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Networking.Packets.Messages;
using Xunit;

namespace NeoServer.Networking.Tests.Packets;

public class NetworkMessageTest
{
    [Fact]
    public void AddLocation_Adding_Struct_Insert_Location_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddLocation(new Location { X = 100, Y = 200, Z = 7 });

        var expected = new byte[5] { 0x64, 0, 0xC8, 0, 7 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddString_Insert_String_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddString("hello world");

        var expected = new byte[]
        {
            11, 0, 0x68, 0x65, 0x6c, 0x6c,
            0x6f, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64
        };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddUInt32_Insert_uint_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddUInt32(400);

        var expected = new byte[] { 0x90, 1, 0, 0 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddUInt16_Insert_ushort_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddUInt16(634);

        var expected = new byte[] { 122, 2 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddUInt16_As_Ushort_Insert_ushort_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddUInt16(784);

        var expected = new byte[] { 16, 3 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddByte_Insert_Byte()
    {
        var sut = new NetworkMessage();
        sut.AddByte(183);

        var expected = new byte[] { 183 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddBytes_Insert_Bytes()
    {
        var sut = new NetworkMessage();
        sut.AddBytes(new byte[] { 194, 12, 13, 0 });

        var expected = new byte[] { 194, 12, 13, 0 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddPaddingBytes_Insert_RightPaddingBytes()
    {
        var sut = new NetworkMessage();
        sut.AddPaddingBytes(6);

        var expected = new byte[6] { 0x33, 0x33, 0x33, 0x33, 0x33, 0x33 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void AddLength_Insert_Length__In_Front_Of_Buffer()
    {
        var sut = new NetworkMessage(new byte[6] { 0x33, 0x33, 0x33, 0x33, 0x33, 0x33 }, 6);
        sut.AddLength();

        var expected = new byte[8] { 6, 0, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33 };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void Adding_Multiple_Types_Insert_All_Bytes_On_Buffer()
    {
        var sut = new NetworkMessage();

        sut.AddByte(0x12);
        sut.AddBytes(new byte[3] { 0x23, 0xB1, 0x00 });
        sut.AddLocation(new Location { X = 432, Y = 343, Z = 10 });
        sut.AddPaddingBytes(3);
        sut.AddString("hello opencoremmo");
        sut.AddUInt16(129);
        sut.AddUInt32(9474);
        sut.AddLength();

        var expected = new byte[39]
        {
            37, 0, 0x12, 0x23, 0xB1, 0x00, 176, 1, 87, 1, 0x0A, 0x33, 0x33, 0x33,
            17, 0, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x6f, 0x70, 0x65, 0x6e, 0x63, 0x6f, 0x72, 0x65, 0x6d, 0x6d,
            0x6f,
            129, 0, 2, 37, 0, 0
        };

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }
}