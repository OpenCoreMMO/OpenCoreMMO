using System;
using System.Security.Cryptography;

namespace NeoServer.Game.Common.Location.Structs.Helpers;

public static class HashHelper
{
    public const int START = 1610612741;
    private static readonly SHA256 Sha256 = SHA256.Create(); // Reused instance

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
    
    public static string ComputeContentHash(ref Span<byte> data)
    {
        Span<byte> hashBytes = stackalloc byte[32]; // SHA256 produces a 256-bit (32-byte) hash
        if (!Sha256.TryComputeHash(data, hashBytes, out _))
        {
            throw new InvalidOperationException("Hash computation failed.");
        }
        return Convert.ToHexString(hashBytes);
    }

}