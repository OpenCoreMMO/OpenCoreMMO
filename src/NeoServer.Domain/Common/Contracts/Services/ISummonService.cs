using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ISummonService
{
    IMonster Summon(ICreature master, string summonName);
}