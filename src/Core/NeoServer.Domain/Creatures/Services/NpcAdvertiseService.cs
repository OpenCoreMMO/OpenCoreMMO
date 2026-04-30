using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Npcs;

namespace NeoServer.Domain.Creatures.Services;

/// <summary>
/// Provides functionality for NPCs to interact with surrounding sociable creatures
/// by broadcasting an advertisement or message within a specific range.
/// </summary>
public class NpcAdvertiseService(IMap map)
{
    /// <summary>
    /// Sends an advertisement message from the NPC to nearby sociable creatures within a specific range.
    /// </summary>
    /// <param name="npc">
    /// The NPC instance initiating the advertisement. Must implement the <see cref="INpc"/> interface.
    /// </param>
    public void SendNpcAdvertise(INpc npc)
    {
        var (maxDistanceX, maxDistanceY) = ((int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortY);

        var spectators = map.GetSpectators(npc.Location, false, false, maxDistanceX, maxDistanceX,
            maxDistanceY, maxDistanceY);
        
        npc.Advertise(spectators.ToList());
    }
}