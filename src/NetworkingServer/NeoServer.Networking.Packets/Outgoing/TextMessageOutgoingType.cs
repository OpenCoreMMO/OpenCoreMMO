namespace NeoServer.Networking.Packets.Outgoing;

public enum TextMessageOutgoingType : byte
{
    MESSAGE_STATUS_CONSOLE_BLUE = 4, /*FIXME Blue message in the console*/

    MESSAGE_STATUS_CONSOLE_RED = 13, /*Red message in the console*/

    MESSAGE_STATUS_DEFAULT = 17, /*White message at the bottom of the game window and in the console*/
    MESSAGE_STATUS_WARNING = 18, /*Red message in game window and in the console*/
    MESSAGE_EVENT_ADVANCE = 19, /*White message in game window and in the console*/

    Small = 21, /*White message at the bottom of the game window"*/
    Description = 22, /*Green message in game window and in the console*/
    MESSAGE_DAMAGE_DEALT = 23,
    MESSAGE_DAMAGE_RECEIVED = Small, //24
    MESSAGE_HEALED = 25,
    MESSAGE_EXPERIENCE = 26,
    MESSAGE_DAMAGE_OTHERS = 27,
    MESSAGE_HEALED_OTHERS = 28,
    MESSAGE_EXPERIENCE_OTHERS = 29,
    MESSAGE_EVENT_DEFAULT = 30, /*White message at the bottom of the game window and in the console*/
    MESSAGE_LOOT = 31,

    MESSAGE_GUILD = 33, /*White message in channel (+ channelId)*/
    MESSAGE_PARTY_MANAGEMENT = 34, /*White message in channel (+ channelId)*/
    MESSAGE_PARTY = 35, /*White message in channel (+ channelId)*/
    MESSAGE_EVENT_ORANGE = 36, /*Orange message in the console*/
    MESSAGE_STATUS_CONSOLE_ORANGE = 37,  /*Orange message in the console*/
}