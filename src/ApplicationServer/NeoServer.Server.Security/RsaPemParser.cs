using System;
using System.Numerics;
using System.Security.Cryptography;

namespace NeoServer.Server.Security;

internal readonly record struct RsaPrivateKeyParameters(
    BigInteger Modulus,
    BigInteger PublicExponent,
    BigInteger PrivateExponent);

internal static class RsaPemParser
{
    private const string Pkcs1Label = "RSA PRIVATE KEY";
    private const byte SequenceTag = 0x30;
    private const byte IntegerTag = 0x02;

    public static RsaPrivateKeyParameters Parse(string pem)
    {
        // OT key.pem uses PKCS#1 with INTEGER sizes RSA.ImportFromPem rejects (p is 65 bytes).
        if (pem.Contains($"BEGIN {Pkcs1Label}", StringComparison.Ordinal))
        {
            return ParsePkcs1(DecodePemBody(pem, Pkcs1Label));
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        var parameters = rsa.ExportParameters(includePrivateParameters: true);

        return new RsaPrivateKeyParameters(
            ToUnsignedBigInteger(parameters.Modulus),
            ToUnsignedBigInteger(parameters.Exponent),
            ToUnsignedBigInteger(parameters.D));
    }

    private static RsaPrivateKeyParameters ParsePkcs1(byte[] encodedKey)
    {
        var offset = 0;
        if (encodedKey.Length == 0 || encodedKey[offset] != SequenceTag)
        {
            throw new InvalidOperationException("Invalid RSA private key PEM.");
        }

        offset++;
        ReadLength(encodedKey, ref offset);

        var index = 0;
        BigInteger modulus = default;
        BigInteger publicExponent = default;
        BigInteger privateExponent = default;

        while (offset < encodedKey.Length && encodedKey[offset] == IntegerTag)
        {
            var value = ReadInteger(encodedKey, ref offset);
            if (index == 1)
            {
                modulus = value;
            }

            if (index == 2)
            {
                publicExponent = value;
            }

            if (index == 3)
            {
                privateExponent = value;
                break;
            }

            index++;
        }

        if (modulus.IsZero || publicExponent.IsZero || privateExponent.IsZero)
        {
            throw new InvalidOperationException("RSA private key PEM is missing required parameters.");
        }

        return new RsaPrivateKeyParameters(modulus, publicExponent, privateExponent);
    }

    private static byte[] DecodePemBody(string pem, string label)
    {
        var header = $"-----BEGIN {label}-----";
        var footer = $"-----END {label}-----";

        var start = pem.IndexOf(header, StringComparison.Ordinal);
        var end = pem.IndexOf(footer, StringComparison.Ordinal);
        if (start < 0 || end < 0 || end <= start)
        {
            throw new InvalidOperationException("Invalid RSA private key PEM.");
        }

        start += header.Length;
        var body = pem.AsSpan(start, end - start);
        var buffer = new char[body.Length];
        var length = 0;

        foreach (var character in body)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            buffer[length] = character;
            length++;
        }

        return Convert.FromBase64String(new string(buffer, 0, length));
    }

    private static BigInteger ReadInteger(byte[] data, ref int offset)
    {
        offset++;
        var length = ReadLength(data, ref offset);
        if (length <= 0 || offset + length > data.Length)
        {
            throw new InvalidOperationException("Invalid RSA private key PEM.");
        }

        var value = new BigInteger(data.AsSpan(offset, length), isUnsigned: true, isBigEndian: true);
        offset += length;
        return value;
    }

    private static int ReadLength(byte[] data, ref int offset)
    {
        if (offset >= data.Length)
        {
            throw new InvalidOperationException("Invalid RSA private key PEM.");
        }

        var lengthByte = data[offset];
        offset++;

        if (lengthByte < 0x80)
        {
            return lengthByte;
        }

        var lengthSize = lengthByte & 0x7F;
        if (lengthSize is 0 or > 4 || offset + lengthSize > data.Length)
        {
            throw new InvalidOperationException("Invalid RSA private key PEM.");
        }

        var length = 0;
        for (var i = 0; i < lengthSize; i++)
        {
            length = (length << 8) | data[offset];
            offset++;
        }

        return length;
    }

    private static BigInteger ToUnsignedBigInteger(byte[] value)
    {
        return new BigInteger(value, isUnsigned: true, isBigEndian: true);
    }
}
