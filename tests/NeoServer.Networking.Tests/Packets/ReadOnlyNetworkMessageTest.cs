using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network.Enums;
using Xunit;

namespace NeoServer.Networking.Tests.Packets;

public class ReadOnlyNetworkMessageShould
{
    [Fact]
    public void Return_Ushort()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);
        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        var expected = new byte[] { 141, 54 };
        Assert.Equal(BitConverter.ToUInt16(expected, 0), sut.GetUInt16());
    }

    [Fact]
    public void Return_Values()
    {
        var data = "1652365658\n\0000006987451230246545648945646";
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        Assert.Equal(BitConverter.ToUInt16(new byte[] { 49, 54 }, 0), sut.GetUInt16());

        Assert.Equal(BitConverter.ToUInt32(new byte[] { 53, 50, 51, 54 }, 0), sut.GetUInt32());

        sut.SkipBytes(3);

        Assert.Equal((byte)56, sut.GetByte());

        var s = sut.GetString();

        Assert.Equal("0000069874", s);
    }

    [Fact]
    public void Increase_BytesRead()
    {
        var data = "1652365658\n\0000006987451230246545648945646";
        var dataBytes = Encoding.ASCII.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        sut.GetUInt16();
        Assert.Equal(2, sut.BytesRead);

        sut.GetUInt32();
        Assert.Equal(6, sut.BytesRead);

        sut.GetByte();
        Assert.Equal(7, sut.BytesRead);

        sut.SkipBytes(3);
        Assert.Equal(10, sut.BytesRead);

        sut.GetString();
        Assert.Equal(22, sut.BytesRead);
    }

    [Fact]
    public void Return_UInt()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);
        Assert.Equal((uint)1652365, sut.GetUInt32());
    }

    [Fact]
    public void SkipBytes()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        sut.SkipBytes(2);

        Assert.Equal(2, sut.BytesRead);
    }

    [Fact]
    public void ThrowException_SkipBytes()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipBytes(20));
    }

    [Fact]
    public void GetByte()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        var expected = (byte)141;
        Assert.Equal(expected, sut.GetByte());
    }

    [Fact]
    public void GetLocation_ReturnsLocation()
    {
        var data = new List<byte>();

        data.AddRange(BitConverter.GetBytes((ushort)1000));
        data.AddRange(BitConverter.GetBytes((ushort)900));
        data.Add(7);

        var sut = new ReadOnlyNetworkMessage(data.ToArray(), data.Count);

        var expected = new Location { X = 1000, Y = 900, Z = 7 };
        Assert.Equal(expected, sut.GetLocation());
    }

    [Fact]
    public void GetBytes()
    {
        var data = 1652365;
        var dataBytes = BitConverter.GetBytes(data);

        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        var expected = new byte[] { 141, 54, 25 };
        Assert.Equal(expected, sut.GetBytes(3).ToArray());
    }

    [Fact]
    public void GetString()
    {
        var data = "\a\0hello world";
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        var expected = "hello w";
        Assert.Equal(expected, sut.GetString());
    }

    [Fact]
    public void ReturnBytes_When_GetMessageInBytes()
    {
        var data = "\a\0hello world";
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var sut = new ReadOnlyNetworkMessage(dataBytes, dataBytes.Length);

        var expected = dataBytes;

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void ReturnEntireBuffer_When_Length_Equals_0_GetMessageInBytes()
    {
        var data = "\a\0hello world";
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var sut = new ReadOnlyNetworkMessage(dataBytes, 0);

        var expected = dataBytes;

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void ReturnSlicedBuffer_When_Length_NotEquals_0_GetMessageInBytes()
    {
        var data = "\a\0hello world";
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var length = 3;
        var sut = new ReadOnlyNetworkMessage(dataBytes, length);

        var expected = dataBytes.Take(length);

        Assert.Equal(expected, sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void GetMessageInBytes_When_Length_Less_Than_0_Returns_Empty()
    {
        var length = -1;
        var sut = new ReadOnlyNetworkMessage(new byte[4], length);

        Assert.Empty(sut.GetMessageInBytes().ToArray());
    }

    [Fact]
    public void Return_None_When_IncomingPacket_Is_Not_Setted()
    {
        var sut = new ReadOnlyNetworkMessage(new byte[4], 4);

        Assert.Equal(GameIncomingPacketType.None, sut.IncomingPacket);
    }

    [Fact]
    public void Return_IncomingPacket_When_Setted()
    {
        var data = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0x01 };

        var sut = new ReadOnlyNetworkMessage(data, 9);

        sut.GetIncomingPacketType(true);

        Assert.Equal(GameIncomingPacketType.PlayerLoginRequest, sut.IncomingPacket);
    }

    [Fact]
    public void Return_IncomingPacket_When_GetIncomingPacketType_Authenticated()
    {
        var data = new byte[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0x14 };

        var sut = new ReadOnlyNetworkMessage(data, 9);

        Assert.Equal(GameIncomingPacketType.PlayerLogOut, sut.GetIncomingPacketType(true));
    }

    [Fact]
    public void Return_IncomingPacket_When_GetIncomingPacketType_Not_Authenticated()
    {
        var data = new byte[9] { 0, 0, 0, 0, 0, 0, 0x14, 0, 0 };

        var sut = new ReadOnlyNetworkMessage(data, 9);

        Assert.Equal(GameIncomingPacketType.PlayerLogOut, sut.GetIncomingPacketType(false));
    }

    [Fact]
    public void GetIncomingPacketType_When_Is_Authenticated_And_Buffer_Less_Than_9_Bytes_Returns_None()
    {
        var data = new byte[3] { 0, 0, 0 };

        var sut = new ReadOnlyNetworkMessage(data, 3);

        Assert.Equal(GameIncomingPacketType.None, sut.GetIncomingPacketType(false));
    }

    [Fact]
    public void GetIncomingPacketType_When_Is_Not_And_Buffer_Less_Than_6_Bytes_Returns_None()
    {
        var data = new byte[3] { 0, 0, 0 };

        var sut = new ReadOnlyNetworkMessage(data, 3);

        Assert.Equal(GameIncomingPacketType.None, sut.GetIncomingPacketType(false));
    }

    [Fact]
    public void Constructor_When_Buffer_Is_Null_Returns()
    {
        var sut = new ReadOnlyNetworkMessage(null, 3);
        Assert.Null(sut.Buffer);
        Assert.Equal(0, sut.Length);
    }

    [Fact]
    public void Resize_When_Length_Less_Than_0_Do_Nothing()
    {
        var sut = new ReadOnlyNetworkMessage(new byte[3], 3);
        sut.Resize(-1);

        Assert.Equal(3, sut.Buffer.Length);
        Assert.Equal(3, sut.Length);
    }

    [Fact]
    public void Return_Set_Length_And_BytesRead_Resize()
    {
        var sut = new ReadOnlyNetworkMessage(new byte[3], 3);

        sut.Resize(5);
        Assert.Equal(5, sut.Length);
        Assert.Equal(0, sut.BytesRead);
    }
}