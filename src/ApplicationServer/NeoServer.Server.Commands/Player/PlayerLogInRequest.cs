using NeoServer.Server.Common.Enums;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogInRequest
{
    public string Account { get; set; }
    public string Password { get; set; }
    public string CharacterName { get; set; }
    public uint[] Xtea { get; set; }
    public byte OtcV8Version { get; set; }
    public OperatingSystem OperatingSystem { get; set; }
    public ushort Version { get; set; }
    public uint ChallengeTimeStamp { get; set; }
    public byte ChallengeNumber { get; set; }
}