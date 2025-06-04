using System;
using System.Buffers.Binary;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Loaders.OTB.DataStructures;

public sealed class ReadOnlyMemoryStream
{
    private readonly ReadOnlyMemory<byte> _buffer;

    /// <summary>
    ///     Creates a new instance of this class
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="position"></param>
    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> buffer, int position = 0)
    {
        if (buffer.IsNull()) return;
        if (position.IsLessThanZero()) return;
        if (position.ThrowIfBiggerThan(buffer.Length)) return;

        _buffer = buffer;
        Position = position;
    }

    public int Position { get; private set; }

    /// <summary>
    ///     Returns true if this instance can read at least 1 more byte.
    ///     Returns false otherwise.
    /// </summary>
    public bool IsOver => Position >= _buffer.Length;

    /// <summary>
    ///     Returns the number of bytes that can still be read.
    /// </summary>
    public int BytesLeftToRead => _buffer.Length - Position;

    /// <summary>
    ///     Returns the value currently pointed by the stream, without moving
    ///     the stream forward.
    /// </summary>
    public byte PeakByte()
    {
        if (IsOver)
            throw new InvalidOperationException();

        return _buffer.Span[Position];
    }

    /// <summary>
    ///     Reads 1 byte from the stream.
    /// </summary>
    public byte ReadByte()
    {
        if (IsOver)
            throw new InvalidOperationException();

        var data = _buffer.Span[Position];
        Position++;
        return data;
    }

    /// <summary>
    ///     Reads two bytes from the stream and parses them as a UInt16.
    /// </summary>
    public ushort ReadUInt16()
    {
        if (BytesLeftToRead < sizeof(ushort))
            throw new InvalidOperationException();

        var parsedData = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Span[Position..(Position + sizeof(ushort))]);
        Position += sizeof(ushort);
        return parsedData;
    }

    /// <summary>
    ///     Reads 4 bytes from the stream and parses them as a UInt32.
    /// </summary>
    public uint ReadUInt32()
    {
        if (BytesLeftToRead < sizeof(uint))
            throw new InvalidOperationException();

        var parsedData = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Span[Position..(Position + sizeof(uint))]);
        Position += sizeof(uint);
        return parsedData;
    }

    /// <summary>
    ///     Moves the stream forward <paramref name="byteCount" /> bytes.
    /// </summary>
    public void Skip(int byteCount = 1)
    {
        if (byteCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        if (BytesLeftToRead < byteCount)
            throw new ArgumentOutOfRangeException(nameof(byteCount));

        Position += byteCount;
    }
}