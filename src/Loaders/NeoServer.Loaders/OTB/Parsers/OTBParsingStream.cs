using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.DataStructures;
using NeoServer.Loaders.OTB.Enums;

namespace NeoServer.Loaders.OTB.Parsers;

public sealed class OtbParsingStream
{
    private readonly ReadOnlyMemoryStream _underlyingStream;

    private byte[] _stringParsingBuffer;

    /// <summary>
    ///     Creates a new instance of <see cref="OtbParsingStream" />.
    /// </summary>
    public OtbParsingStream(ReadOnlyMemory<byte> otbData)
    {
        _underlyingStream = new ReadOnlyMemoryStream(otbData);

        // Initial buffer for string parsing only
        _stringParsingBuffer = new byte[256];
    }

    public int CurrentPosition => _underlyingStream.Position;

    /// <summary>
    ///     Returns true if there are no bytes left to read.
    ///     Returns false otherwise.
    /// </summary>
    public bool IsOver => _underlyingStream.IsOver;

    /// <summary>
    ///     Returns true if stream can read next given bytes
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool CanReadNextBytes(int count)
    {
        return _underlyingStream.BytesLeftToRead >= count;
    }

    /// <summary>
    ///     Reads a byte from the underlaying stream, considering OTB's escape values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        var value = _underlyingStream.ReadByte();

        if ((OtbMarkupByte)value != OtbMarkupByte.Escape)
            return value;
        return _underlyingStream.ReadByte();
    }

    /// <summary>
    ///     Reads a byte and converts it to a bool using C++ rules.
    /// </summary>
    public bool ReadBool()
    {
        var value = ReadByte();
        return value != 0;
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a UInt16.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        for (var i = 0; i < sizeof(ushort); i++)
            buffer[i] = ReadByte();

        return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
    }

    public Coordinate ReadCoordinate()
    {
        var x = ReadUInt16();
        var y = ReadUInt16();
        var z = (sbyte)ReadByte();

        return new Coordinate(x, y, z);
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a UInt32.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        for (var i = 0; i < sizeof(uint); i++)
            buffer[i] = ReadByte();

        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a UInt64.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        for (var i = 0; i < sizeof(ulong); i++)
            buffer[i] = ReadByte();

        return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
    }

    /// <summary>
    ///     Reads bytes from the underlying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a double.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        for (var i = 0; i < sizeof(double); i++)
            buffer[i] = ReadByte();

        return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
    }

    /// <summary>
    ///     Reads a byte from the underlying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a ASCII-encoded string.
    ///     The first 2 bytes read (considering OTB's escape values) represent the string length.
    /// </summary>
    public string ReadString()
    {
        var stringLength = ReadUInt16();

        // Use stack allocation for small strings, heap for larger ones
        if (stringLength <= 256)
        {
            Span<byte> buffer = stackalloc byte[stringLength];
            for (var i = 0; i < stringLength; i++)
                buffer[i] = ReadByte();
            
            return Encoding.ASCII.GetString(buffer);
        }

        // "Resize" our buffer, iff necessary
        if (stringLength > _stringParsingBuffer.Length)
            _stringParsingBuffer = new byte[stringLength];

        for (var i = 0; i < stringLength; i++)
            _stringParsingBuffer[i] = ReadByte();

        // When in C land, use C encoding...
        return Encoding.ASCII.GetString(_stringParsingBuffer, 0, stringLength);
    }

    /// <summary>
    ///     Skips <paramref name="byteCount" /> bytes from the underlaying stream, considering OTB's escape values.
    /// </summary>
    public void Skip(int byteCount = 1)
    {
        if (byteCount <= 0)
            throw new ArgumentOutOfRangeException();

        for (var i = 0; i < byteCount; i++)
            ReadByte();
    }
}