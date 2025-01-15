using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Creatures.Monster.Managers;

namespace NeoServer.Server.Routines.Creatures.Monster;

public static class MonsterStateRoutine
{
    public static void Execute(IMonster monster, ISummonService summonService)
    {
        MonsterStateManager.Run(monster, summonService);
    }
}