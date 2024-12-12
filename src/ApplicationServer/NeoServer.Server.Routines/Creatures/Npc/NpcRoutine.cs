using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Creatures.Npc;

public class NpcRoutine
{
    private static readonly IntervalControl Interval = new(3_000);

    public static void Execute(INpc npc)
    {
        if (!Interval.CanExecuteNow()) return;

        npc.Advertise();
        npc.WalkRandomStep();

        Interval.MarkAsExecuted();
    }
}