using System;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network.Enums;

namespace NeoServer.Server.Common.Contracts.Network;

// <summary>
/// Interface for read-only network messages.
/// </summary>
/// <remarks>
///     This interface provides methods for reading and parsing incoming network messages in a read-only manner.
/// </remarks>
public interface IReadOnlyNetworkMessage
{
    /// <summary>
    ///     Gets the buffer containing the message.
    /// </summary>
    byte[] Buffer { get; }

    /// <summary>
    ///     Gets the length of the message.
    /// </summary>
    int Length { get; }

    /// <summary>
    ///     Gets the number of bytes that have been read from the message.
    /// </summary>
    int BytesRead { get; }

    /// <summary>
    ///     Gets the type of incoming packet.
    /// </summary>
    GameIncomingPacketType IncomingPacket { get; }

    /// <summary>
    ///     Reads a 16-bit unsigned integer from the message.
    /// </summary>
    /// <returns>The 16-bit unsigned integer.</returns>
    ushort GetUInt16();

    /// <summary>
    ///     Reads a 32-bit unsigned integer from the message.
    /// </summary>
    /// <returns>The 32-bit unsigned integer.</returns>
    uint GetUInt32();

    /// <summary>
    ///     Reads a 64-bit unsigned integer from the message.
    /// </summary>
    /// <returns>The 64-bit unsigned integer.</returns>
    ulong GetUInt64();

    /// <summary>
    ///     Skips a specified number of bytes in the message.
    /// </summary>
    /// <param name="count">The number of bytes to skip.</param>
    void SkipBytes(int count);

    /// <summary>
    ///     Reads a byte from the message.
    /// </summary>
    /// <returns>The byte.</returns>
    byte GetByte();

    /// <summary>
    ///     Reads a specified number of bytes from the message.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>A read-only span of the bytes read.</returns>
    ReadOnlySpan<byte> GetBytes(int length);

    /// <summary>
    ///     Reads a string from the message.
    /// </summary>
    /// <returns>The string.</returns>
    string GetString();

    /// <summary>
    ///     Reads a double from the message.
    /// </summary>
    /// <returns>The string.</returns>
    double GetDouble();

    /// <summary>
    ///     Gets a read-only span of the message bytes.
    /// </summary>
    /// <returns>A read-only span of the message bytes.</returns>
    ReadOnlySpan<byte> GetMessageInBytes();

    /// <summary>
    ///     Gets the type of incoming packet, based on whether the connection is authenticated or not.
    /// </summary>
    /// <param name="isAuthenticated">True if the connection is authenticated, otherwise false.</param>
    /// <returns>The type of incoming packet.</returns>
    GameIncomingPacketType GetIncomingPacketType(bool isAuthenticated);

    /// <summary>
    ///     Resizes the message to a specified length.
    /// </summary>
    /// <param name="length">The new length of the message.</param>
    void Resize(int length);

    /// <summary>
    ///     Resets the message to its initial state.
    /// </summary>
    void Reset();

    /// <summary>
    ///     Reads a location from the message.
    /// </summary>
    /// <returns>The location.</returns>
    Location GetLocation();
}