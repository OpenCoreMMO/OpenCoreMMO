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
        
        message.SkipBytes(7); 

        OperatingSystem = (OperatingSystem)message.GetUInt16();
        Version = message.GetUInt16();

        message.SkipBytes(7); // U32 client version, U8 client type, U16 dat revision

        //// todo: version validation

        var encryptedData = message.GetBytes(Rsa.LENGTH);

        var decryptedData = Rsa.Decrypt(encryptedData.ToArray());
        if (decryptedData is null || decryptedData.Length == 0) return;

        var data = new ReadOnlyNetworkMessage(decryptedData, encryptedData.Length);

        LoadXtea(data);

        GameMaster = Convert.ToBoolean(data.GetByte());
        var sessionKey = data.GetString();

        if (string.IsNullOrEmpty(sessionKey))
        {
            //todo: 1098 disconnect();
            return;
        }

        var sessionArgs = sessionKey.Split('\n');
        if (sessionArgs.Length != 4)
        {
            //todo: 1098 disconnect();
            return;
        }

        Account = sessionArgs[0];
        Password = sessionArgs[1];
        var token = sessionArgs[2];

        CharacterName = data.GetString();

        //todo: 1098 implement this
        //if (challengeTimestamp != timeStamp || challengeRandom != randNumber)
        //{
        //    disconnect();
        //    return;
        //}

        ChallengeTimeStamp = data.GetUInt32();
        ChallengeNumber = data.GetByte();
        var clientStringLength = data.GetUInt16();
        if (clientStringLength == 5 && data.GetString(5) == "OTCv8") OtcV8Version = data.GetUInt16();
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