using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IOutfit
{
    ushort LookType { get; set; }
    byte Head { get; set; }
    byte Body { get; set; }
    byte Legs { get; set; }
    byte Feet { get; set; }
    byte Addon { get; set; }

    public Gender Type { get; protected set; }
    public string Name { get; protected set; }
    public bool Premium { get; protected set; }
    public bool Unlocked { get; protected set; }
    public bool Enabled { get; protected set; }

    void Change(ushort lookType, byte head, byte body, byte legs, byte feet, byte addon);
    IOutfit Clone();

    public IOutfit SetName(string name)
    {
        Name = name;
        return this;
    }

    public IOutfit SetPremium(bool premium)
    {
        Premium = premium;
        return this;
    }

    public IOutfit SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return this;
    }

    public IOutfit SetGender(Gender type)
    {
        Type = type;
        return this;
    }

    public IOutfit SetUnlocked(bool unlocked)
    {
        Unlocked = unlocked;
        return this;
    }
}