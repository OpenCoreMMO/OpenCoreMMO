using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.World.Events;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.World;

public class CreatureMovedEventHandler(IMoveEvents moveEvents) : IApplicationEventHandler<CreatureMovedInTheMap>
{
    public void Handle(CreatureMovedInTheMap @event)
    {
        var cylinder = @event.Cylinder;
        var creature = @event.Creature;

        if (cylinder.IsNull()) return;
        if (cylinder.TileSpectators.IsNull()) return;
        if (creature.IsNull()) return;

        var toTile = cylinder.ToTile;
        var fromTile = cylinder.FromTile;
        if (toTile.IsNull()) return;
        if (fromTile.IsNull()) return;

        moveEvents.OnCreatureMove(creature, cylinder.FromTile.Location, cylinder.ToTile.Location);
    }
}