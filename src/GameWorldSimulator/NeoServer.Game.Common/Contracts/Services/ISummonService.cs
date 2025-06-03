using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.Services;

public interface ISummonService
{
    IMonster Summon(ICreature master, string summonName);
}