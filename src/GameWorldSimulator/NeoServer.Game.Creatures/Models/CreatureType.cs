using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Creatures.Models;

public class CreatureType : ICreatureType
{
    public CreatureType(string name, string description, uint health, uint maxHealth, ushort speed,
        IDictionary<LookType, ushort> look)
    {
        Name = name;
        Description = description;
        Health = health;
        MaxHealth = maxHealth;
        Speed = speed;
        Look = look;
    }

    public string Description { get; set; }
    public string Name { get; set; }
    public uint Health { get; set; }
    public uint MaxHealth { get; set; }
    public ushort Speed { get; set; }
    public IDictionary<LookType, ushort> Look { get; set; }
}