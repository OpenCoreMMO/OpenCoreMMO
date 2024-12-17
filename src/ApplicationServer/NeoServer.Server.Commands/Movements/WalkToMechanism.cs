using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Commands.Movements;

public class WalkToMechanism : IWalkToMechanism
{
    private readonly IScheduler _scheduler;

    public WalkToMechanism(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void WalkTo(IPlayer player, Action action, Location toLocation, bool secondChance = false)
    {
        if (!toLocation.IsNextTo(player.Location))
        {
            if (secondChance) return;

            player.WalkTo(toLocation, CallBack);
            return;

            void CallBack(ICreature _) =>
                _scheduler.AddEvent(
                    new SchedulerEvent(player.StepDelay, () => WalkTo(player, action, toLocation, true)));
        }

        action?.Invoke();
    }
}