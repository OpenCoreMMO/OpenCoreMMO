using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using NeoServer.Domain.Common.Location.Structs.Helpers;

namespace NeoServer.Benchmarks.Hashing;

/// <summary>
///     Compares the old SHA-256 tile-cache key approach (string-allocated) with
///     the new dual-algorithm 128-bit hash (FNV-1a + DJB2, allocation-free).
///     Input sizes reflect realistic OTBM tile payloads: each item contributes
///     2 bytes (client-id, little-endian), so N items → N*2 bytes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(3)]
public class TileHashBenchmark
{
    // Tile sizes that represent typical OTBM tiles (1, 5, and 10 items on a tile)
    [Params(2, 10, 20)]
    public int PayloadBytes { get; set; }

    private byte[] _data;

    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[PayloadBytes];
        new Random(42).NextBytes(_data);
    }

    /// <summary>
    ///     Old approach: SHA-256 over the raw bytes, then hex-encode to a string.
    ///     Allocates a 32-byte hash array and a 64-character string on every call.
    /// </summary>
    [Benchmark(Baseline = true, Description = "SHA-256 + ToHexString (old)")]
    public string OldSha256HexString()
    {
        var hashBytes = SHA256.HashData(_data);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    ///     New approach: FNV-1a + DJB2 running in a single pass over a Span&lt;byte&gt;.
    ///     Zero heap allocations; returns a (ulong, ulong) value-tuple used directly
    ///     as the tile-cache dictionary key.
    /// </summary>
    [Benchmark(Description = "FNV-1a + DJB2 128-bit (new)")]
    public (ulong Low, ulong High) NewFnvDjb2Hash()
    {
        var span = _data.AsSpan();
        return HashHelper.ComputeContentHash(ref span);
    }
}
