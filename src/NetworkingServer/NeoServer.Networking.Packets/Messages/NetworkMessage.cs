using System;
using System.Buffers;
using System.Text;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Security;

namespace NeoServer.Networking.Packets.Messages;

/// <summary>
///     Contains all the methods to handle incoming and outgoing message from/to client
/// </summary>
public class NetworkMessage : ReadOnlyNetworkMessage, INetworkMessage
{
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
    private int _cursor;

    public NetworkMessage(byte[] buffer, int length) : base(buffer, length)
    {
        Length = length;
        _cursor = length;
    }

    public NetworkMessage(int length = 0) : base(new byte[16394], length)
    {
        Length = length;
        _cursor = length;
    }

    public override void Reset()
    {
        base.Reset();
        _cursor = 0;
    }

    /// <summary>
    ///     Inserts a location point on the buffer
    /// </summary>
    /// <param name="location"></param>
    public void AddLocation(Location location)
    {
        AddUInt16(location.X);
        AddUInt16(location.Y);
        AddByte(location.Z);
    }

    /// <summary>
    ///     Inserts a string on buffer. This method put the string's length in front of the value itself
    ///     Ex: the string "hello world" will be inserted as 0x0b 0 + string bytes
    /// </summary>
    /// <param name="value"></param>
    public void AddString(string value)
    {
        AddUInt16((ushort)value.Length);
        WriteBytes(Encoding.Latin1.GetBytes(value));
    }

    /// <summary>
    ///     Inserts item object into the buffer.
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(IItem item)
    {
        if (item == null)
            //todo log
            return;

        AddBytes(item.GetRaw().ToArray());
    }

    /// <summary>
    ///     Adds uint value (4 bytes) to buffer
    /// </summary>
    /// <param name="value"></param>
    public void AddUInt32(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BitConverter.TryWriteBytes(buffer, value);
        WriteBytes(buffer);
    }

    public void WriteUint32(uint value, int position)
    {
        Span<byte> buffer = stackalloc byte[4];
        BitConverter.TryWriteBytes(buffer, value);
        
        Buffer[position] = buffer[position++];
        Buffer[position] = buffer[position++];
        Buffer[position] = buffer[position++];
        Buffer[position] = buffer[position];
    }

    /// <summary>
    ///     Adds ushort (2 bytes) to buffer
    /// </summary>
    /// <param name="value"></param>
    public void AddUInt16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BitConverter.TryWriteBytes(buffer, value);
        WriteBytes(buffer);
    }

    /// <summary>
    ///     Adds a byte value to buffer
    /// </summary>
    /// <param name="b"></param>
    public void AddByte(byte b)
    {
        WriteByte(b);
    }

    /// <summary>
    ///     Adds a array of bytes to buffer
    /// </summary>
    /// <param name="bytes"></param>
    public void AddBytes(ReadOnlySpan<byte> bytes)
    {
        var buffer = BufferPool.Rent(bytes.Length);
        bytes.CopyTo(buffer);
        AddBytes(buffer, bytes.Length);
        BufferPool.Return(buffer);
    }

    /// <summary>
    ///     Adds padding right bytes (0x33) to buffer
    /// </summary>
    /// <param name="count">how many times the padding will be added</param>
    public void AddPaddingBytes(int count)
    {
        WriteBytes(0x33, count);
    }

    /// <summary>
    ///     Add header bytes and return the whole packet
    /// </summary>
    /// <param name="addChecksum"></param>
    /// <returns></returns>
    public byte[] AddHeader(bool addChecksum = true)
    {
        Span<byte> newArray = stackalloc byte[Length + 6];

        var header = GetHeader();

        Buffer.AsSpan(0, Length).CopyTo(newArray.Slice(6));
        header.AsSpan().CopyTo(newArray);

        return newArray.ToArray();
    }

    /// <summary>
    ///     Add payload length to the buffer
    ///     The ushort bytes will be added in front of buffer
    /// </summary>
    public void AddLength()
    {
        var srcBuffer = Buffer.AsSpan(0, Length);
        Span<byte> newArray = new byte[Length + 50]; //added 50 for xtea padding

        var lengthBytes = BitConverter.GetBytes((ushort)Length);
        newArray[0] = lengthBytes[0];
        newArray[1] = lengthBytes[1];

        srcBuffer.CopyTo(newArray.Slice(2, Length));

        Length += 2;
        _cursor += 2;
        Buffer = newArray.ToArray();
    }

    private void AddBytes(byte[] bytes, int length)
    {
        Array.Copy(bytes, 0, Buffer, Length, length);
        Length += length;
        _cursor += length;
    }

    private void WriteBytes(Span<byte> bytes)
    {
        for (var i = 0; i < bytes.Length; i++) WriteByte(bytes[i]);
    }

    private void WriteBytes(byte b, int times)
    {
        for (var i = 0; i < times; i++) WriteByte(b);
    }

    private void WriteByte(byte b)
    {
        Length++;
        Buffer[_cursor++] = b;
    }
    
    public void SetBufferPosition(int position) => _cursor = position;

    private byte[] GetHeader(bool addChecksum = true)
    {
        var checkSumBytes =
            addChecksum ? BitConverter.GetBytes(AdlerChecksum.Checksum(Buffer, 0, Length)) : new byte[4];
        var lengthInBytes = BitConverter.GetBytes((ushort)(Length + checkSumBytes.Length));
        var header = new byte[6];

        // Direct array assignments instead of BlockCopy
        header[0] = lengthInBytes[0];
        header[1] = lengthInBytes[1];
        header[2] = checkSumBytes[0];
        header[3] = checkSumBytes[1];
        header[4] = checkSumBytes[2];
        header[5] = checkSumBytes[3];

        return header;
    }
}