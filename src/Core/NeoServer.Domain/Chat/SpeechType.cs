namespace NeoServer.Domain.Chat;

public enum SpeechType : byte
{
    None = 0,
    Say = 1,
    Whisper = 2,
    Yell = 3,
    PrivateFrom = 4,
    PrivateTo = 5,
    ChannelManager = 6,
    ChannelYellow = 7,
    ChannelOrange = 8,
    PrivateNpcToPlayer = 10,
    PrivatePlayerToNpc = 11,
    GamemasterBroadcast = 12,       // Blue color for administrators
    GamemasterChannel = 13,         // Light blue color for community managers  
    ChannelRed1 = 14,               // red - #c text
    PrivateRedFrom = 15,            // @name@text
    PrivateRedTo = 16,              // @name@text
    MonsterSay = 36,
    MonsterYell = 37,
    MessageBlue = 46,               // Blue color alternative
    ChannelRed2 = 255               // #d text - second red color for differentiation
}