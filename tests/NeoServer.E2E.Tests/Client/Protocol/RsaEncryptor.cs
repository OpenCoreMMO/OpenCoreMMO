using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal static class RsaEncryptor
{
    private const int CiphertextSize = 128;
    private const int MaxPlaintextSize = CiphertextSize - 1;

    public static byte[] Encrypt(ReadOnlySpan<byte> data, string dataPath)
    {
        if (data.Length > MaxPlaintextSize)
        {
            throw new InvalidOperationException(
                $"RSA payload length {data.Length} exceeds maximum {MaxPlaintextSize}.");
        }

        using var reader = File.OpenText(Path.Combine(dataPath, "key.pem"));
        var keyPair = (Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

        var block = new byte[MaxPlaintextSize];
        data.CopyTo(block);

        var paddingLength = MaxPlaintextSize - data.Length - 1;
        for (var index = 0; index < paddingLength; index++)
        {
            block[data.Length + index] = 0x33;
        }

        block[MaxPlaintextSize - 1] = 0x00;

        var engine = new RsaEngine();
        engine.Init(true, keyPair.Public);

        return engine.ProcessBlock(block, 0, MaxPlaintextSize);
    }
}
