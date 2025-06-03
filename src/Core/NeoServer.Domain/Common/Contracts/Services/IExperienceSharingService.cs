using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IExperienceSharingService
{
    void Share(ICreature creature);
}