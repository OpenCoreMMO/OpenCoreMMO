namespace NeoServer.Domain.Common.Location;

public class MapConstants
{
    public const byte MAX_DISTANCE_MOVE_THING = 11;
    public const byte DEFAULT_MAP_WINDOW_SIZE_X = 18;
    public const byte DEFAULT_MAP_WINDOW_SIZE_Y = 14;

    public const int MAX_VIEW_IN_X = 11; // min value: maxClientViewportX + 1
    public const int MAX_VIEW_IN_Y = 11; // min value: max
    public const int MAX_CLIENT_VIEWPORT_X = 8;
    public const int MAX_CLIENT_VIEWPORT_Y = 6;
    public const int LIMIT_OF_OBJECTS_ON_TILE = 9;
}