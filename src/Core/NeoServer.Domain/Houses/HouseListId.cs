namespace NeoServer.Domain.Houses;

public static class HouseListId
{
    public const uint GuestList = 0x100;
    
    public const uint SubOwnerList = 0x101;

    /// <summary>Which door this access list controls. Values 0-254 are door ids.</summary>
    public static bool IsDoorList(uint listId) => listId <= 254;
}
