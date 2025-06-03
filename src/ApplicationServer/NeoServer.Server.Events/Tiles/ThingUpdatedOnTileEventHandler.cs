using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Tiles;

public class ThingUpdatedOnTileEventHandler
{
    private readonly IGameServer game;

    public ThingUpdatedOnTileEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IThing thing, ICylinder cylinder)
    {
        if (Guard.AnyNull(cylinder, cylinder.TileSpectators, thing)) return;

        var tile = cylinder.ToTile;
        if (tile.IsNull()) return;

        foreach (var spectator in cylinder.TileSpectators)
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.Spectator.CreatureId, out var connection))
                continue;

            if (!spectator.Spectator.CanSee(thing.Location)) continue;

            connection.OutgoingPackets.Enqueue(new UpdateTileItemPacket(thing.Location, spectator.ToStackPosition,
                (IItem)thing));

            connection.Send();
        }
    }
}