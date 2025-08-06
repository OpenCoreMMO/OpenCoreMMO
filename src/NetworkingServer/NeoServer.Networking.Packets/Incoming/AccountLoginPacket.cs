using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using NeoServer.Server.Security;

namespace NeoServer.Networking.Packets.Incoming;

public class AccountLoginPacket : IncomingPacket
{
    public AccountLoginPacket(IReadOnlyNetworkMessage message)
    {
        message.SkipBytes(7);
        OperatingSystem = (OperatingSystem)message.GetUInt16();
        ProtocolVersion = message.GetUInt16();

        message.SkipBytes(17);

        var encryptedData = message.GetBytes(Rsa.LENGTH);
        var bytes = Rsa.Decrypt(encryptedData.ToArray());

        if (bytes is null || bytes.Length == 0) return;

        var data = new ReadOnlyNetworkMessage(bytes, encryptedData.Length);

        LoadXtea(data);

        Account = data.GetString();
        Password = data.GetString();

        // read authenticator token and stay logged in flag from last 128 bytes
        message.SkipBytes((message.Length - Rsa.LENGTH) - message.BytesRead); 
        encryptedData = message.GetBytes(Rsa.LENGTH);
        bytes = Rsa.Decrypt(encryptedData.ToArray());
        data = new ReadOnlyNetworkMessage(bytes, encryptedData.Length);

        Token = data.GetString();
    }

    public OperatingSystem OperatingSystem { get; set; }

    public string Account { get; }
    public string Password { get; }
    public ushort ProtocolVersion { get; }
    public string Token { get; }

    public bool IsValid()
    {
        return !(string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(Password));
    }
}