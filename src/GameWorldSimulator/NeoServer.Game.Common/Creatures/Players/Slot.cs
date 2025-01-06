namespace NeoServer.Game.Common.Creatures.Players;

public enum Slot : byte
{
    None = 0x00,
    Head = 0x01,
    Necklace = 0x02,
    Backpack = 0x03,
    Body = 0x04,
    Right = 0x05,
    Left = 0x06,
    Legs = 0x07,
    Feet = 0x08,
    Ring = 0x09,
    Ammo = 0x0A,
    Depot = 0x0B,
    TwoHanded = 0x0C,
    Hand = (Left | Right),
}