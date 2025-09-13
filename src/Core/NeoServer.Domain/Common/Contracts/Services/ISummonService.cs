using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ISummonService
{
    IMonster SpamSummon(ICreature master, string summonName);
}