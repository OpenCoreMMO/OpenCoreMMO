namespace NeoServer.Domain.Common;

public static class GameConstants
{
    public const int MAX_NUMBER_OF_ITEMS_ON_INBOX = 30;

    public const ushort PARCEL_SERVER_ID = 2595;
    public const ushort STAMPED_PARCEL_SERVER_ID = 2596;

    public const ushort LABEL_SERVER_ID = 2599;

    public const ushort LETTER_SERVER_ID = 2597;
    public const ushort STAMPED_LETTER_SERVER_ID = 2598;

    //stamina
    public const int STAMINA_MAX_MINUTES = 42 * 60; //42 hours
    public const int STAMINA_BONUS_MINUTES = 39 * 60; //39 hours - +50% exp gain
    public const int STAMINA_THRESHOLD_MINUTES = 14 * 60; //14 hours - -50% exp gain and no loot
    public const int STAMINA_BONUS_EXP_PERCENTAGE = 50;
    public const int STAMINA_THRESHOLD_EXP_PERCENTAGE = -50;
    public const int STAMINA_REGENERATION_EACH_MINUTES = 3; //recover 1 minute of stamina each 3 minutes 
}