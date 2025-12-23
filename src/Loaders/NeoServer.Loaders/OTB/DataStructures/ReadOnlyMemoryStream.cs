using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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
        if (position < 0) return;
        if (position > buffer.Length) return;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte PeakByte()
    {
        if (Position >= _buffer.Length)
            throw new InvalidOperationException();

        return _buffer.Span[Position];
    }

    /// <summary>
    ///     Reads 1 byte from the stream.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        if (Position >= _buffer.Length)
            throw new InvalidOperationException();

        return _buffer.Span[Position++];
    }

    /// <summary>
    ///     Reads two bytes from the stream and parses them as a UInt16.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        var newPosition = Position + sizeof(ushort);
        if (newPosition > _buffer.Length)
            throw new InvalidOperationException();

        var result = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Span.Slice(Position, sizeof(ushort)));
        Position = newPosition;
        return result;
    }

    /// <summary>
    ///     Reads 4 bytes from the stream and parses them as a UInt32.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        var newPosition = Position + sizeof(uint);
        if (newPosition > _buffer.Length)
            throw new InvalidOperationException();

        var result = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Span.Slice(Position, sizeof(uint)));
        Position = newPosition;
        return result;
    }

    /// <summary>
    ///     Moves the stream forward <paramref name="byteCount" /> bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Skip(int byteCount = 1)
    {
        var newPosition = Position + byteCount;
        if (byteCount <= 0 || newPosition > _buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(byteCount));

        Position = newPosition;
    }
}