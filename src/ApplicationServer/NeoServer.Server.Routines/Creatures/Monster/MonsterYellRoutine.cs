using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Creatures.Monster;

public static class MonsterYellRoutine
{
    public static void Execute(IMonster monster)
    {
        if (monster.IsDead) return;

        monster.Yell();
    }
}