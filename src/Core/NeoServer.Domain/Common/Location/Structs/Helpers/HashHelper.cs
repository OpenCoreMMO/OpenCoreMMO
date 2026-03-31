namespace NeoServer.Domain.Common.Location.Structs.Helpers;

public static class HashHelper
{
    public const int START = 1610612741;

    // FNV-1a 64-bit constants
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

    // DJB2 seed (second independent pass)
    private const ulong Djb2Seed = 5381UL;

    /// <summary>
    ///     Combines the current hashcode with the hashcode of another object.
    /// </summary>
    public static int CombineHashCode<T>(this int hashCode, T arg)
    {
        unchecked
        {
            return 16777619 * hashCode + arg.GetHashCode();
        }
    }

    /// <summary>
    ///     Computes a 128-bit content hash over the supplied byte span.
    ///     Two structurally independent 64-bit algorithms (FNV-1a and DJB2) run
    ///     in a single pass, making collision probability negligible (~2^-128).
    ///     Not cryptographic - intended for cache keying only.
    /// </summary>
    public static (ulong Low, ulong High) ComputeContentHash(ref Span<byte> data)
    {
        var h1 = FnvOffsetBasis; // FNV-1a
        var h2 = Djb2Seed;      // DJB2

        foreach (var b in data)
        {
            // FNV-1a: XOR then multiply
            h1 ^= b;
            h1 *= FnvPrime;

            // DJB2: shift-add then XOR
            h2 = ((h2 << 5) + h2) ^ b;
        }

        return (h1, h2);
    }
}