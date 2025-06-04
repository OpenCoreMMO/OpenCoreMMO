using System;
using System.Text;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.DataStructures;
using NeoServer.Loaders.OTB.Enums;

namespace NeoServer.Loaders.OTB.Parsers;

public sealed class OtbParsingStream
{
    private readonly ReadOnlyMemoryStream _underlyingStream;

    private byte[] _parsingBuffer;

    /// <summary>
    ///     Creates a new instance of <see cref="OtbParsingStream" />.
    /// </summary>
    public OtbParsingStream(ReadOnlyMemory<byte> otbData)
    {
        _underlyingStream = new ReadOnlyMemoryStream(otbData);

        // The buffer must be at least as big as the largest non-string
        // object we can parse. Currently it's a UInt64.
        _parsingBuffer = new byte[sizeof(ulong)];
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
    public ushort ReadUInt16()
    {
        for (var i = 0; i < sizeof(ushort); i++)
            _parsingBuffer[i] = ReadByte();

        return BitConverter.ToUInt16(_parsingBuffer, 0);
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
    public uint ReadUInt32()
    {
        for (var i = 0; i < sizeof(uint); i++)
            _parsingBuffer[i] = ReadByte();

        return BitConverter.ToUInt32(_parsingBuffer, 0);
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a UInt64.
    /// </summary>
    public ulong ReadUInt64()
    {
        for (var i = 0; i < sizeof(ulong); i++)
            _parsingBuffer[i] = ReadByte();

        return BitConverter.ToUInt64(_parsingBuffer, 0);
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a double.
    /// </summary>
    public double ReadDouble()
    {
        for (var i = 0; i < sizeof(double); i++)
            _parsingBuffer[i] = ReadByte();

        return BitConverter.ToDouble(_parsingBuffer, 0);
    }

    /// <summary>
    ///     Reads a bytes from the underlaying stream, considering OTB's escape values,
    ///     until enough bytes were read to parse them as a ASCII-encoded string.
    ///     The first 2 bytes read (considering OTB's escape values) represent the string length.
    /// </summary>
    public string ReadString()
    {
        var stringLength = ReadUInt16();

        // "Resize" our buffer, iff necessary
        if (stringLength > _parsingBuffer.Length)
            _parsingBuffer = new byte[stringLength];

        for (var i = 0; i < stringLength; i++)
            _parsingBuffer[i] = ReadByte();

        // When in C land, use C encoding...
        return Encoding.ASCII.GetString(_parsingBuffer, 0, stringLength);
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