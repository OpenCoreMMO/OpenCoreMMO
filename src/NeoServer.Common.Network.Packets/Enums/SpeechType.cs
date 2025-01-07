namespace NeoServer.Common.Network.Packets.Enums;

public enum SpeechType : byte
{
    None = 0,
    Say = 1,
    Whisper = 2,
    Yell = 3,
    PrivatePlayerToNpc = 4,
    PrivateNpcToPlayer = 5,
    Private = 6,
    ChannelYellowText = 7,
    ChannelWhiteText = 8,
    RvrChannel = 9,
    RvrAnswer = 10,
    RvrContinue = 11,
    Broadcast = 12,
    ChannelRed1Text = 13, //red - #c text
    PrivateRed = 14, //@name@text
    ChannelOrangeText = 15, //@name@text
    ChannelRed2Text = 17, //#d
    MonsterSay = 19,
    MonsterYell = 20
}
