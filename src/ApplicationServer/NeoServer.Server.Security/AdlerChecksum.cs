namespace NeoServer.Server.Security;

public static class AdlerChecksum
{
    public static uint Checksum(byte[] data, int index, int length)
    {
        const ushort adler = 65521;

        uint a = 1, b = 0;

        while (length > 0)
        {
            var tmp = length > 5552 ? 5552 : length;
            length -= tmp;

            do
            {
                a += data[index++];
                b += a;
            } while (--tmp > 0);

            a %= adler;
            b %= adler;
        }

        return (b << 16) | a;
    }
}