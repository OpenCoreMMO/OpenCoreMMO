namespace NeoServer.Domain.Common.Location.Structs.Helpers;

public static class HashHelper
{
    public const int START = 1610612741;

    // FNV-1a 64-bit constants
    private const ulong FnvOffsetBasis = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;

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
    ///     Computes a fast 64-bit FNV-1a hash over the supplied byte span.
    ///     Not cryptographic - intended for cache keying only.
    /// </summary>
    public static ulong ComputeContentHash(ref Span<byte> data)
    {
        var hash = FnvOffsetBasis;
        foreach (var b in data)
        {
            hash ^= b;
            hash *= FnvPrime;
        }

        return hash;
    }
}