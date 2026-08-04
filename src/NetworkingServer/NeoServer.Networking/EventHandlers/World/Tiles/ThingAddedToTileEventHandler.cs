using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.World.Events;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Networking.EventHandlers.World.Tiles;

public class ThingAddedToTileEventHandler(IGameServer game, IScriptManager scriptManager)
    : INetworkingEventHandler<ThingAddedToTileEvent>
{
    public void Handle(ThingAddedToTileEvent @event)
    {
        Execute(@event.Thing, @event.Cylinder);
    }

    private void Execute(IThing thing, ICylinder cylinder)
    {
        if (Guard.AnyNull(cylinder, cylinder.TileSpectators, thing)) return;
        var tile = cylinder.ToTile;
        if (tile.IsNull()) return;

        var spectators = cylinder.TileSpectators;

        foreach (var spectator in spectators)
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.Spectator.CreatureId, out var connection))
                continue;

            if (spectator.Spectator is not IPlayer player) continue;

            if (!player.CanSee(thing.Location)) continue;

            connection.OutgoingPackets.Enqueue(new AddTileItemPacket((IItem)thing, spectator.ToStackPosition));

            connection.Send();
        }

        scriptManager.MoveEvents.ItemMove(thing as IItem, cylinder.ToTile, true);
    }
}