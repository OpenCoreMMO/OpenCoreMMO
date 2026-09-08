using System;
using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using NeoServer.Server.Security;
using Xunit;

namespace NeoServer.Server.Tests.Security;

public class RsaTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void Rsa_decrypts_payload_when_encrypted_with_tibia_padding()
    {
        var keyDirectory = CreateTempKeyDirectory();
        try
        {
            Rsa.LoadPem(keyDirectory);

            var payload = new byte[] { 0x10, 0x20, 0x30, 0x40, 0x50 };
            var encrypted = Rsa.Encrypt(payload);

            var decrypted = Rsa.Decrypt(encrypted);

            decrypted.Should().NotBeNull();
            decrypted.Should().HaveCount(127);
            decrypted.AsSpan(0, payload.Length).ToArray().Should().Equal(payload);
            decrypted[payload.Length].Should().Be(0x33);
            decrypted[^1].Should().Be(0x00);
        }
        finally
        {
            Directory.Delete(keyDirectory, recursive: true);
        }
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Rsa_encrypts_to_128_byte_ciphertext()
    {
        var keyDirectory = CreateTempKeyDirectory();
        try
        {
            Rsa.LoadPem(keyDirectory);

            var encrypted = Rsa.Encrypt([0x01, 0x02, 0x03]);

            encrypted.Should().HaveCount(Rsa.LENGTH);
        }
        finally
        {
            Directory.Delete(keyDirectory, recursive: true);
        }
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Rsa_decrypts_legacy_bouncycastle_ciphertext_from_default_key()
    {
        var dataPath = ResolveDataPath();
        File.Exists(Path.Combine(dataPath, "key.pem")).Should().BeTrue();
        Rsa.LoadPem(dataPath);

        var payload = new byte[] { 0x10, 0x20, 0x30, 0x40, 0x50 };
        var ciphertext = Convert.FromHexString(
            "4B83336C57B62B6F3B6E477326899C218513B5FF1F94CFC1418E5C402C321757" +
            "F2AAD8B9F105FA00633EA73C17275287ECEDE144755C4673F402FA3F40F788CC" +
            "28E928D8C85775E269709220A6BEB563384E383540D1DA6BC5B48E216A534FEB" +
            "0CECE4C2EE6AB1947583A6F9CF9AFD00D5604002CEC03CE647FDE9D625CF6CE4");

        var decrypted = Rsa.Decrypt(ciphertext);

        decrypted.Should().NotBeNull();
        decrypted.Should().HaveCount(127);
        decrypted.AsSpan(0, payload.Length).ToArray().Should().Equal(payload);
        decrypted[payload.Length].Should().Be(0x33);
        decrypted[^1].Should().Be(0x00);
        Rsa.Encrypt(payload).Should().Equal(ciphertext);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Rsa_roundtrips_with_default_server_key_pem()
    {
        var dataPath = ResolveDataPath();
        File.Exists(Path.Combine(dataPath, "key.pem")).Should().BeTrue();

        Rsa.LoadPem(dataPath);

        var payload = "account-login"u8.ToArray();
        var decrypted = Rsa.Decrypt(Rsa.Encrypt(payload));

        decrypted.Should().NotBeNull();
        decrypted.AsSpan(0, payload.Length).ToArray().Should().Equal(payload);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Rsa_returns_null_when_ciphertext_is_null()
    {
        var decrypted = Rsa.Decrypt(null);

        decrypted.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Rsa_returns_null_when_ciphertext_is_longer_than_block()
    {
        var keyDirectory = CreateTempKeyDirectory();
        try
        {
            Rsa.LoadPem(keyDirectory);

            var decrypted = Rsa.Decrypt(new byte[Rsa.LENGTH + 1]);

            decrypted.Should().BeNull();
        }
        finally
        {
            Directory.Delete(keyDirectory, recursive: true);
        }
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Rsa_throws_when_encrypt_payload_exceeds_maximum()
    {
        var keyDirectory = CreateTempKeyDirectory();
        try
        {
            Rsa.LoadPem(keyDirectory);

            var act = () => Rsa.Encrypt(new byte[128]);

            act.Should().Throw<InvalidOperationException>();
        }
        finally
        {
            Directory.Delete(keyDirectory, recursive: true);
        }
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Rsa_decrypts_full_block_to_exactly_127_bytes()
    {
        var dataPath = ResolveDataPath();
        Rsa.LoadPem(dataPath);

        var ciphertext = new byte[Rsa.LENGTH];
        ciphertext[^1] = 0x01;

        var decrypted = Rsa.Decrypt(ciphertext);

        decrypted.Should().NotBeNull();
        decrypted.Should().HaveCount(127);
    }

    private static string ResolveDataPath()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data"));
    }

    private static string CreateTempKeyDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "opencoremmo-rsa-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        using var rsa = RSA.Create(1024);
        File.WriteAllText(Path.Combine(directory, "key.pem"), rsa.ExportRSAPrivateKeyPem());
        return directory;
    }
}
