using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;

namespace NeoServer.Game.Systems.Houses;

public class House
{
    public IEnumerable<ITile> Tiles { get; set; }
    public IPlayer Owner { get; set; }
    public IEnumerable<IPlayer> Guests { get; set; }
    public IDictionary<int, IEnumerable<IPlayer>> DoorsAccess { get; set; }
}