using System;
using NeoServer.Networking.Packets.Messages;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Security;
using OperatingSystem = NeoServer.Server.Common.Enums.OperatingSystem;

namespace NeoServer.Networking.Packets.Incoming;

public class PlayerLogInPacket : IncomingPacket
{
    public PlayerLogInPacket(IReadOnlyNetworkMessage message)
    {
        var packetLength = message.GetUInt16();
        var tcpPayload = packetLength + 2;
        message.SkipBytes(5);

        OperatingSystem = (OperatingSystem) message.GetUInt16();
        Version = message.GetUInt16();

        //message.SkipBytes(9);

        //// todo: version validation

        var encryptedDataLength = tcpPayload - message.BytesRead;
        var encryptedData = message.GetBytes(encryptedDataLength);

        var decryptedData = Rsa.Decrypt(encryptedData.ToArray());
        if (decryptedData is null || decryptedData.Length == 0) return;

        var data = new ReadOnlyNetworkMessage(decryptedData, encryptedDataLength);

        LoadXtea(data);

        GameMaster = Convert.ToBoolean(data.GetByte());
        Account = data.GetString();
        CharacterName = data.GetString();
        Password = data.GetString();
        ChallengeTimeStamp = data.GetUInt32();
        ChallengeNumber = data.GetByte();
        var clientStringLength = data.GetUInt16();
        if (clientStringLength == 5 && data.GetString(5) == "OTCv8")
        {
            OtcV8Version = data.GetUInt16();
        }
    }

    public ushort OtcV8Version { get; set; }
    
    public string Account { get; set; }
    public string Password { get; set; }
    public string CharacterName { get; set; }
    public bool GameMaster { get; set; }
    public uint ChallengeTimeStamp { get; set; }
    public byte ChallengeNumber { get; set; }
    public OperatingSystem OperatingSystem { get; set; }
    public ushort Version { get; set; }
}