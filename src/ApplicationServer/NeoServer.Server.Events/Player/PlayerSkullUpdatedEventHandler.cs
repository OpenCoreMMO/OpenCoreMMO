using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerSkullUpdatedEventHandler(IMap map, IGameCreatureManager creatureManager) : IEventHandler
{
    public void Execute(IPlayer player)
    {
        foreach (var spectator in map.GetPlayersAtPositionZone(player.Location))
        {
            if(spectator is not IPlayer spectatorPlayer) continue;

            if (!creatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            
            connection.Send(new PlayerSkullPacket
            {
                Player = player,
                Spectator = spectatorPlayer
            });
        }
    }
}