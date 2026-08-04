using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Services;

namespace NeoServer.Server.Routines.Creatures.Npc;

public class NpcRoutine(NpcAdvertiseService npcAdvertiseService): IRoutine
{
    private static readonly IntervalControl Interval = new(3_000);

    public void Execute(INpc npc)
    {
        if (!Interval.CanExecuteNow()) return;

        npcAdvertiseService.SendNpcAdvertise(npc);
        npc.WalkRandomStep();

        Interval.MarkAsExecuted();
    }
}