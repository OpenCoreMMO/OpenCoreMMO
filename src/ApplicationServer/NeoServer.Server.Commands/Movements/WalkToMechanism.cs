using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
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

            void CallBack(ICreature _)
            {
                //todo: 1098 resting new speed calculation
                //_scheduler.AddEvent(
                //    new SchedulerEvent(player.GetStepDelay(player.Location, toLocation), () => WalkTo(player, action, toLocation, true)));

                _scheduler.AddEvent(
                    new SchedulerEvent(player.GetStepDelay(), () => WalkTo(player, action, toLocation, true)));
            }
        }

        action?.Invoke();
    }
}