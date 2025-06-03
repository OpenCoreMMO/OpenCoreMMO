using NeoServer.Domain.World;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Routines.World;

public class GameWorldRoutine(IGameServer game, Domain.World.World world)
{
    public void StartChecking()
    {
        game.Scheduler.AddEvent(new SchedulerEvent(WorldLight.EVENT_WORLD_LIGHT_INTERVAL, StartChecking));
        world.WorldLight.UpdateWorldLightState();
    }
}