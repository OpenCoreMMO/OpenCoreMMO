using System.Collections.Generic;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface ICreatureType
{
    string Name { get; set; }
    string Description { get; set; }
    uint Health { get; set; }
    uint MaxHealth { get; set; }
    ushort Speed { get; set; }
    IDictionary<LookType, ushort> Look { get; set; }
}