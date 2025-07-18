namespace NeoServer.Domain.Creatures.Player.Inventory;

public enum Slot : byte
{
    None = 0,
    Head = 1,
    Necklace = 2,
    Backpack = 3,
    Body = 4,
    Right = 5,
    Left = 6,
    Legs = 7,
    Feet = 8,
    Ring = 9,
    Ammo = 10,
    Depot = 11,
    TwoHanded = 12,
    Hand = Left | Right
}