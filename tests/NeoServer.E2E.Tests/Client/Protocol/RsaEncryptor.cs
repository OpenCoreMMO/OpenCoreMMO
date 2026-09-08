using NeoServer.Server.Security;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal static class RsaEncryptor
{
    public static byte[] Encrypt(ReadOnlySpan<byte> data, string dataPath)
    {
        Rsa.LoadPem(dataPath);
        return Rsa.Encrypt(data);
    }
}
