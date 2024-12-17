using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Items;

public class ItemUsedOnTileEventHandler
{
    private readonly IGameCreatureManager _creatureManager;
    private readonly IMap _map;

    public ItemUsedOnTileEventHandler(IGameCreatureManager creatureManager, IMap map)
    {
        _creatureManager = creatureManager;
        _map = map;
    }

    public void Execute(ICreature usedBy, ITile onTile, IUsableOnTile item)
    {
        foreach (var spectator in _map.GetPlayersAtPositionZone(usedBy.Location))
        {
            if (!_creatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (item.Metadata.ShootType != default)
                connection.OutgoingPackets.Enqueue(new DistanceEffectPacket(usedBy.Location, onTile.Location,
                    (byte)item.Metadata.ShootType));
            connection.Send();
        }
    }
}