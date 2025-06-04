using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Data.Entities;

public class PlayerOutfitAddonEntity
{
    public PlayerEntity Player { get; set; }
    public int PlayerId { get; set; }
    public int LookType { get; set; }
    public OutfitAddon AddonLevel { get; set; } = OutfitAddon.None;
}