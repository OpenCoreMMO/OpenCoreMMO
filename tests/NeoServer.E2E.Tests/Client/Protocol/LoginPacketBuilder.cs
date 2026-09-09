using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network.Enums;

namespace NeoServer.E2E.Tests.Client.Protocol;

internal static class LoginPacketBuilder
{
    private const ushort ProtocolVersion = 860;

    public static (byte[] Packet, uint[] XteaKey) BuildAccountLogin(
        string account,
        string password)
    {
        var xteaKey = GenerateXteaKey();
        var rsaPayload = BuildAccountRsaPayload(xteaKey, account, password);
        var encryptedRsa = RsaEncryptor.Encrypt(rsaPayload);

        var message = new NetworkMessage();
        message.AddByte((byte)GameIncomingPacketType.PlayerLoginRequest);
        message.AddUInt16((ushort)NeoServer.Server.Common.Enums.OperatingSystem.Windows);
        message.AddUInt16(ProtocolVersion);
        message.AddBytes(new byte[12]);
        message.AddBytes(encryptedRsa);

        return (ProtocolFraming.BuildClientPacket(message), xteaKey);
    }

    public static (byte[] Packet, uint[] XteaKey) BuildGameLogin(
        string account,
        string password,
        string characterName,
        uint challengeTimeStamp,
        byte challengeNumber)
    {
        var xteaKey = GenerateXteaKey();
        var rsaPayload = BuildGameRsaPayload(
            xteaKey,
            account,
            password,
            characterName,
            challengeTimeStamp,
            challengeNumber);
        var encryptedRsa = RsaEncryptor.Encrypt(rsaPayload);

        var message = new NetworkMessage();
        message.AddByte((byte)GameIncomingPacketType.PlayerLogIn);
        message.AddUInt16((ushort)NeoServer.Server.Common.Enums.OperatingSystem.Windows);
        message.AddUInt16(ProtocolVersion);
        message.AddBytes(encryptedRsa);

        return (ProtocolFraming.BuildClientPacket(message), xteaKey);
    }

    private static byte[] BuildAccountRsaPayload(uint[] xteaKey, string account, string password)
    {
        var rsaMessage = new NetworkMessage();
        rsaMessage.AddUInt32(xteaKey[0]);
        rsaMessage.AddUInt32(xteaKey[1]);
        rsaMessage.AddUInt32(xteaKey[2]);
        rsaMessage.AddUInt32(xteaKey[3]);
        rsaMessage.AddString(account);
        rsaMessage.AddString(password);

        return rsaMessage.GetMessageInBytes().ToArray();
    }

    private static byte[] BuildGameRsaPayload(
        uint[] xteaKey,
        string account,
        string password,
        string characterName,
        uint challengeTimeStamp,
        byte challengeNumber)
    {
        var rsaMessage = new NetworkMessage();
        rsaMessage.AddUInt32(xteaKey[0]);
        rsaMessage.AddUInt32(xteaKey[1]);
        rsaMessage.AddUInt32(xteaKey[2]);
        rsaMessage.AddUInt32(xteaKey[3]);
        rsaMessage.AddByte(0);
        rsaMessage.AddString(account);
        rsaMessage.AddString(characterName);
        rsaMessage.AddString(password);
        rsaMessage.AddUInt32(challengeTimeStamp);
        rsaMessage.AddByte(challengeNumber);

        return rsaMessage.GetMessageInBytes().ToArray();
    }

    private static uint[] GenerateXteaKey()
    {
        return
        [
            (uint)Random.Shared.Next(),
            (uint)Random.Shared.Next(),
            (uint)Random.Shared.Next(),
            (uint)Random.Shared.Next()
        ];
    }
}
