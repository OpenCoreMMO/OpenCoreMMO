using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Server.Common.Contracts.Scripts.Services;

public interface IMoveEventsScriptService
{
    void ItemMove(IItem item, ITile tile, bool isAdd);
    void CreatureMove(ICreature creature, Location fromLocation, Location toLocation);
}