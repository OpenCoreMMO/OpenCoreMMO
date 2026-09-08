using System;
using System.IO;
using System.Numerics;

namespace NeoServer.Server.Security;

public static class Rsa
{
    public const int LENGTH = 128;
    private const int PlaintextLength = LENGTH - 1;
    private const byte PaddingFillByte = 0x33;

    private static BigInteger _modulus;
    private static BigInteger _publicExponent;
    private static BigInteger _privateExponent;

    public static byte[] Decrypt(byte[] data)
    {
        try
        {
            if (data is null || data.Length > LENGTH)
            {
                return null;
            }

            var cipher = new BigInteger(data, isUnsigned: true, isBigEndian: true);
            var plain = BigInteger.ModPow(cipher, _privateExponent, _modulus);
            return ConvertDecryptOutput(plain);
        }
        catch
        {
            return null;
        }
    }

    public static byte[] Encrypt(ReadOnlySpan<byte> data)
    {
        if (data.Length > PlaintextLength)
        {
            throw new InvalidOperationException(
                $"RSA payload length {data.Length} exceeds maximum {PlaintextLength}.");
        }

        var block = new byte[PlaintextLength];
        data.CopyTo(block);

        var paddingLength = PlaintextLength - data.Length - 1;
        for (var index = 0; index < paddingLength; index++)
        {
            block[data.Length + index] = PaddingFillByte;
        }

        block[PlaintextLength - 1] = 0x00;

        var plain = new BigInteger(block, isUnsigned: true, isBigEndian: true);
        var cipher = BigInteger.ModPow(plain, _publicExponent, _modulus);
        return ConvertEncryptOutput(cipher);
    }

    public static void LoadPem(string basePath)
    {
        var pem = File.ReadAllText(Path.Combine(basePath, "key.pem"));
        var key = RsaPemParser.Parse(pem);

        _modulus = key.Modulus;
        _publicExponent = key.PublicExponent;
        _privateExponent = key.PrivateExponent;
    }

    private static byte[] ConvertDecryptOutput(BigInteger value)
    {
        return ToFixedLength(value, PlaintextLength);
    }

    private static byte[] ConvertEncryptOutput(BigInteger value)
    {
        return ToFixedLength(value, LENGTH);
    }

    private static byte[] ToFixedLength(BigInteger value, int length)
    {
        var output = value.ToByteArray(isUnsigned: true, isBigEndian: true);

        if (output.Length == length)
        {
            return output;
        }

        if (output.Length > length)
        {
            var trimmed = new byte[length];
            Buffer.BlockCopy(output, output.Length - length, trimmed, 0, length);
            return trimmed;
        }

        var padded = new byte[length];
        Buffer.BlockCopy(output, 0, padded, length - output.Length, output.Length);
        return padded;
    }
}
