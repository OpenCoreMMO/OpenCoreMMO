using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Monster.Services;

namespace NeoServer.Server.Routines.Creatures.Monster;

public class MonsterYellRoutine(MonsterYellService monsterYellService): IRoutine
{
    public void Execute(IMonster monster)
    {
        if (monster.IsDead) return;

        monsterYellService.Yell(monster);
    }
}