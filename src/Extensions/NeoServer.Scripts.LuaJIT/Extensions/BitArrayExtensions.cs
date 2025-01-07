using System.Collections;

namespace NeoServer.Scripts.LuaJIT.Extensions
{
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Converts a BitArray to an unsigned long (ulong).
        /// </summary>
        /// <param name="bitArray">The BitArray to be converted.</param>
        /// <returns>The value of the BitArray as a ulong.</returns>
        /// <exception cref="ArgumentException">Thrown when the BitArray exceeds 64 bits.</exception>
        public static ulong ToULong(this BitArray bitArray)
        {
            if (bitArray.Length > 64)
                throw new ArgumentException("The BitArray cannot have more than 64 bits to be converted to a ulong.");

            ulong result = 0;
            for (int i = 0; i < bitArray.Length; i++)
                if (bitArray[i])
                    result |= (1UL << i);

            return result;
        }
    }
}
