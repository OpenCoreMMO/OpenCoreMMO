using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Benchmarks.Networking;

[SimpleJob(RunStrategy.ColdStart, 30)]
public class XteaBenchmark
{
    private readonly uint[] keys = [2742731963, 828439173, 895464428, 91929452];

    private INetworkMessage GetNetworkMessage()
    {
        var input = new NetworkMessage();
        input.AddString("abcdefgh");
        return input;
    }

    [Benchmark]
    public INetworkMessage WithoutSpan()
    {
        return Xtea.Encrypt(GetNetworkMessage(), keys);
    }

    [Benchmark]
    public INetworkMessage WithSpan()
    {
        return Xtea.EncryptWithSpan(GetNetworkMessage(), keys);
    }
}

public class Xtea
{
    public static INetworkMessage Encrypt(INetworkMessage msg, uint[] key)
    {
        if (key == null)
            throw new ArgumentException("Key invalid");

        var pad = msg.Length % 8;
        if (pad > 0) msg.AddPaddingBytes(8 - pad);

        var words = Split(msg.GetMessageInBytes().ToArray()).ToArray();

        for (var pos = 0; pos < msg.Length / 4; pos += 2)
        {
            uint x_sum = 0, x_delta = 0x9e3779b9, x_count = 32;

            while (x_count-- > 0)
            {
                words[pos] += (((words[pos + 1] << 4) ^ (words[pos + 1] >> 5)) + words[pos + 1]) ^ (x_sum
                    + key[x_sum & 3]);
                x_sum += x_delta;
                words[pos + 1] += (((words[pos] << 4) ^ (words[pos] >> 5)) + words[pos]) ^ (x_sum
                    + key[(x_sum >> 11) & 3]);
            }
        }

        var newBytes = words.SelectMany(x => BitConverter.GetBytes(x)).ToArray();

        return new NetworkMessage(newBytes, msg.Length);
    }

    public static INetworkMessage EncryptWithSpan(INetworkMessage msg, uint[] key)
    {
        if (key == null)
            throw new ArgumentException("Key invalid");

        var pad = msg.Length % 8;
        if (pad > 0) msg.AddPaddingBytes(8 - pad);

        var words = NewSplit(msg.Buffer.AsSpan(0, msg.Length));

        for (var pos = 0; pos < msg.Length / 4; pos += 2)
        {
            uint x_sum = 0, x_delta = 0x9e3779b9, x_count = 32;

            while (x_count-- > 0)
            {
                words[pos] += (((words[pos + 1] << 4) ^ (words[pos + 1] >> 5)) + words[pos + 1]) ^ (x_sum
                    + key[x_sum & 3]);
                x_sum += x_delta;
                words[pos + 1] += (((words[pos] << 4) ^ (words[pos] >> 5)) + words[pos]) ^ (x_sum
                    + key[(x_sum >> 11) & 3]);
            }
        }

        var newBytes = ConvertToBytes(words); //words.SelectMany(x => BitConverter.GetBytes(x)).ToArray();

        return new NetworkMessage(newBytes, msg.Length);
    }

    private static byte[] ConvertToBytes(Span<uint> array)
    {
        var bytes = new byte[array.Length * 4];
        var index = 0;
        for (var i = 0; i < array.Length; i++)
        {
            var newBytes = BitConverter.GetBytes(array[i]);

            bytes[index] = newBytes[0];
            bytes[index + 1] = newBytes[1];
            bytes[index + 2] = newBytes[2];
            bytes[index + 3] = newBytes[3];

            index += 4;
        }

        return bytes;
    }

    private static IEnumerable<uint> Split(byte[] array)
    {
        for (var i = 0; i < array.Length; i += sizeof(uint))
        {
            var to = i + 4;
            yield return BitConverter.ToUInt32(array[i..to], 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<uint> NewSplit(ReadOnlySpan<byte> array)
    {
        var newArray = new uint[array.Length / 4];

        var index = 0;
        for (var i = 0; i < array.Length; i += sizeof(uint))
        {
            newArray[index] = BitConverter.ToUInt32(array.Slice(i, 4));
            index++;
        }

        return newArray.AsSpan();
    }
}