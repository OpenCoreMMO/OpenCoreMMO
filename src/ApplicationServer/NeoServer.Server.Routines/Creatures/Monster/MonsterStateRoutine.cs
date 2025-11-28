using NeoServer.Domain.Creatures.Monster.Services;

namespace NeoServer.Server.Routines.Creatures.Monster;

public class MonsterStateRoutine(MonsterStateService monsterStateService) : IRoutine
{
    public void Execute(IMonster monster)
    {
        monsterStateService.UpdateState(monster);
    }
}