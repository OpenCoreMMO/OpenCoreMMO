using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Players;

namespace NeoServer.Domain.Creatures.Player;

public class Outfit : IOutfit
{
    public ushort LookType { get; set; }

    public byte Head { get; set; }

    public byte Body { get; set; }

    public byte Legs { get; set; }

    public byte Feet { get; set; }
    public byte Addon { get; set; }

    Gender IOutfit.Type { get; set; }

    string IOutfit.Name { get; set; }

    bool IOutfit.Premium { get; set; }

    bool IOutfit.Unlocked { get; set; }

    bool IOutfit.Enabled { get; set; }

    public void Change(ushort lookType, byte head, byte body, byte legs, byte feet, byte addon)
    {
        LookType = lookType;
        Head = head;
        Body = body;
        Legs = legs;
        Feet = feet;
        Addon = addon;
    }

    public IOutfit Clone()
    {
        return (IOutfit)MemberwiseClone();
    }
}