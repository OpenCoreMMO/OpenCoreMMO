using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.World.Events;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.World;

public class ItemMovedToTrashHolderEventHandler(IMap map, IGameCreatureManager gameCreatureManager)
    : INetworkingEventHandler<ItemMovedToTrashHolder>
{
    public void Handle(ItemMovedToTrashHolder @event)
    {
        var spectators = map.GetSpectators(@event.Tile.Location, onlyPlayers: true);

        foreach (var spectator in spectators)
        {
            if (spectator is not IPlayer player) continue;

            if (!gameCreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) continue;

            // If the tile is a liquid source, show a blue ring effect to the player
            if (@event.Tile.HasFlag(TileFlags.LiquidSource))
            {
                connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Tile.Location, EffectT.RingsBlue));
                connection.Send();
            }
        }
    }
}
