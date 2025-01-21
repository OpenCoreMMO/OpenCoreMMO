using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IMoveEventsScriptService
{
    void ItemMove(IItem item, ITile tile, bool isAdd);
    void CreatureMove(ICreature creature, Location fromLocation, Location toLocation);
}