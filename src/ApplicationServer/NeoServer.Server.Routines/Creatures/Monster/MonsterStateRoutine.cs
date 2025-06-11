using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Creatures.Monster.Services;

namespace NeoServer.Server.Routines.Creatures.Monster;

public static class MonsterStateRoutine
{
    public static void Execute(IMonster monster, ISummonService summonService)
    {
        MonsterStateService.Run(monster, summonService);
    }
}