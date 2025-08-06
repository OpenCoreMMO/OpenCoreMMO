namespace NeoServer.Domain.Chat;

public enum SpeechType : byte
{
    None = 0,
    Say = 1,
    Whisper = 2,
    Yell = 3,
    PrivateFrom = 4,
    PrivateTo = 5,
    ChannelYellow = 7,
    ChannelOrange = 8,
    PrivateNpcToPlayer = 10,
    PrivatePlayerToNpc = 12,
    Broadcast = 13,
    ChannelRed1 = 14, // red - #c text
    PrivateRedFrom = 15, // @name@text
    PrivateRedTo = 16,   // @name@text
    MonsterSay = 36,
    MonsterYell = 37
}