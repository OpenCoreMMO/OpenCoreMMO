namespace NeoServer.Domain.Creatures.Player.Outfit;

public class Outfit
{
    public ushort LookType { get; set; }

    public byte Head { get; set; }

    public byte Body { get; set; }

    public byte Legs { get; set; }

    public byte Feet { get; set; }
    public byte Addon { get; set; }

    public Gender Type { get; private set; }

    public string Name { get; private set; }

    public bool Premium { get; private set; }

    public bool Unlocked { get; private set; }

    public bool Enabled { get; private set; }

    public void Change(ushort lookType, byte head, byte body, byte legs, byte feet, byte addon)
    {
        LookType = lookType;
        Head = head;
        Body = body;
        Legs = legs;
        Feet = feet;
        Addon = addon;
    }

    public Outfit SetName(string name)
    {
        Name = name;
        return this;
    }

    public Outfit SetPremium(bool premium)
    {
        Premium = premium;
        return this;
    }

    public Outfit SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    public Outfit SetGender(Gender type)
    {
        Type = type;
        return this;
    }

    public Outfit SetUnlocked(bool unlocked)
    {
        Unlocked = unlocked;
        return this;
    }

    public Outfit Clone()
    {
        return (Outfit)MemberwiseClone();
    }
}