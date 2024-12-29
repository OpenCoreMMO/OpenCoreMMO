using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Server.Services;

public static class EffectService
{
    public static void Send(Location location, EffectT effect, IEnumerable<ICreature> spectators = null)
    {
        var map = IoC.GetInstance<IMap>();
        var game = IoC.GetInstance<IGameServer>();

        if (spectators == null || !spectators.Any())
            spectators = map.GetPlayersAtPositionZone(location);

        foreach (var spectator in spectators)
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(location, effect));
            connection.Send();
        }
    }
}