using System;
using System.Runtime.InteropServices;
using System.Text;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Network.Enums;

namespace NeoServer.Networking.Packets.Messages;

public class ReadOnlyNetworkMessage : IReadOnlyNetworkMessage
{
    private static readonly byte[] EmptyBuffer = Array.Empty<byte>();
    private static readonly Encoding Iso88591Encoding = Encoding.GetEncoding("iso-8859-1");

    public ReadOnlyNetworkMessage(byte[] buffer, int length)
    {
        if (buffer.IsNull()) return;

        Buffer = buffer;
        Length = length;
        BytesRead = 0;
    }

    public byte[] Buffer { get; protected set; }
    public int Length { get; protected set; }

    /// <summary>
    ///     Get the message's buffer
    /// </summary>
    public ReadOnlySpan<byte> GetMessageInBytes()
    {
        return Length.IsLessThanZero() ? EmptyBuffer : Length == 0 ? Buffer : Buffer[..Length];
    }

    public int BytesRead { get; private set; }

    public GameIncomingPacketType IncomingPacket { get; private set; } = GameIncomingPacketType.None;

    public GameIncomingPacketType GetIncomingPacketType(bool isAuthenticated)
    {
        switch (isAuthenticated)
        {
            case true:
                if (Buffer.Length.IsLessThan(9)) return GameIncomingPacketType.None;
                SkipBytes(6); //skip header
                GetUInt16();
                var packetType = (GameIncomingPacketType)GetByte();
                IncomingPacket = packetType;
                return packetType;

            case false:
                if (Buffer.Length.IsLessThan(6)) return GameIncomingPacketType.None;
                IncomingPacket = (GameIncomingPacketType)Buffer[6];
                return IncomingPacket;

            default:
                throw new ArgumentException("Unexpected value for isAuthenticated");
        }
    }

    public ushort GetUInt16()
    {
        return Convert(BitConverter.ToUInt16);
    }

    public uint GetUInt32()
    {
        return Convert(BitConverter.ToUInt32);
    }

    public void SkipBytes(int length)
    {
        if (length + BytesRead > Buffer.Length)
            throw new ArgumentOutOfRangeException("Cannot skip bytes that exceeds the buffer length");
        IncreaseByteRead(length);
    }

    public byte GetByte()
    {
        return Convert((_, _) => Buffer[BytesRead]);
    }

    public ReadOnlySpan<byte> GetBytes(int length)
    {
        var to = BytesRead + length;

        if (to > Buffer.Length) length = Buffer.Length - BytesRead;

        var result = Buffer.AsSpan(BytesRead, length);
        IncreaseByteRead(length);
        return result;
    }

    /// <summary>
    ///     Get string value based on payload length
    /// </summary>
    /// <returns></returns>
    public string GetString()
    {
        var length = GetUInt16();
        if (length == 0 || BytesRead + length > Buffer.Length) return null;

        var span = GetBytes(length);
        return Iso88591Encoding.GetString(span);
    }

    public void Resize(int length)
    {
        if (length.IsLessThanZero()) return;

        Length = length;
        BytesRead = 0;
    }

    public virtual void Reset()
    {
        Buffer = new byte[16394];
        BytesRead = 0;
        Length = 0;
    }

    public Location GetLocation()
    {
        return new Location { X = GetUInt16(), Y = GetUInt16(), Z = GetByte() };
    }

    /// <summary>
    ///     Get string value based on payload length
    /// </summary>
    /// <returns></returns>
    public string GetString(int length)
    {
        if (length == 0 || BytesRead + length > Buffer.Length) return null;

        var span = GetBytes(length);
        return Iso88591Encoding.GetString(span);
    }

    private void IncreaseByteRead(int length)
    {
        BytesRead += length;
    }

    private static int SizeOf<T>() where T : struct
    {
        return Marshal.SizeOf(default(T));
    }

    private T Convert<T>(Func<byte[], int, T> converter) where T : struct
    {
        var result = converter(Buffer, BytesRead);
        IncreaseByteRead(SizeOf<T>());
        return result;
    }
}