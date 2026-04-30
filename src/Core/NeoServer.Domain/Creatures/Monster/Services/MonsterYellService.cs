using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Services;

namespace NeoServer.Domain.Creatures.Monster.Services;

public class MonsterYellService(ICreatureSpeechService creatureSpeechService)
{
    public void Yell(IMonster monster)
    {
        var spectators = creatureSpeechService.GetYellSpectators(monster);
        monster.Yell(spectators);
    }
}