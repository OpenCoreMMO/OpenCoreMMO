using NeoServer.Networking.Packets.Messages;
using NeoServer.Networking.Packets.Security;
using NeoServer.Server.Security;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal static class ProtocolFraming
{
    public static byte[] BuildClientPacket(NetworkMessage message)
    {
        return message.AddHeader();
    }

    public static byte[] BuildEncryptedGamePacket(NetworkMessage message, uint[] xteaKey)
    {
        message.AddLength();
        Xtea.Encrypt(message, xteaKey);

        return message.AddHeader();
    }

    public static async Task<byte[]> ReadLengthPrefixedPacketAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var sizeHeader = new byte[2];
        await ReadExactAsync(stream, sizeHeader, cancellationToken);

        var messageSize = BitConverter.ToUInt16(sizeHeader, 0) + 2;
        var buffer = new byte[messageSize];
        sizeHeader.CopyTo(buffer, 0);

        if (messageSize > 2)
        {
            await ReadExactAsync(stream, buffer, 2, messageSize - 2, cancellationToken);
        }

        return buffer;
    }

    public static async Task<byte[]> ReadServerPacketAsync(
        Stream stream,
        uint[] xteaKey,
        bool decryptAtIndexSix,
        CancellationToken cancellationToken)
    {
        var header = new byte[6];
        await ReadExactAsync(stream, header, cancellationToken);

        var payloadLength = BitConverter.ToUInt16(header, 0) - 4;
        var payload = new byte[payloadLength];
        await ReadExactAsync(stream, payload, 0, payloadLength, cancellationToken);

        if (xteaKey is null)
        {
            return payload;
        }

        var message = new NetworkMessage(payload, payload.Length);
        var decryptIndex = decryptAtIndexSix ? 6 : 0;
        Xtea.Decrypt(message, decryptIndex, xteaKey);

        return message.Buffer.AsSpan(0, message.Length).ToArray();
    }

    public static async Task ReadExactAsync(
        Stream stream,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        await ReadExactAsync(stream, buffer, 0, buffer.Length, cancellationToken);
    }

    public static async Task ReadExactAsync(
        Stream stream,
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;

        while (totalRead < count)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead), cancellationToken);

            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Connection closed before the expected number of bytes was received.");
            }

            totalRead += bytesRead;
        }
    }

    public static (uint TimeStamp, byte RandomNumber) ParseChallengePacket(byte[] buffer)
    {
        var message = new ReadOnlyNetworkMessage(buffer, buffer.Length);
        message.SkipBytes(2);
        message.SkipBytes(4);
        message.SkipBytes(2);
        message.SkipBytes(1);

        return (message.GetUInt32(), message.GetByte());
    }

    public static byte ReadOpcode(byte[] decryptedPayload)
    {
        var message = new ReadOnlyNetworkMessage(decryptedPayload, decryptedPayload.Length);
        message.SkipBytes(2);
        return message.GetByte();
    }

    public static bool ContainsOpcode(byte[] decryptedPayload, byte opcode)
    {
        for (var index = 0; index < decryptedPayload.Length; index++)
        {
            if (decryptedPayload[index] == opcode)
            {
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<string> ParseCharacterNames(byte[] decryptedPayload)
    {
        var message = new ReadOnlyNetworkMessage(decryptedPayload, decryptedPayload.Length);
        message.SkipBytes(2);
        message.GetByte();
        var characterCount = message.GetByte();
        var names = new List<string>(characterCount);

        for (var index = 0; index < characterCount; index++)
        {
            names.Add(message.GetString());
            message.GetString();
            message.SkipBytes(4);
            message.GetUInt16();
        }

        return names;
    }
}
