using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ICreatureType
{
    string Name { get; set; }
    string Description { get; set; }
    uint Health { get; set; }
    uint MaxHealth { get; set; }
    ushort Speed { get; set; }
    IDictionary<LookType, ushort> Look { get; set; }
}