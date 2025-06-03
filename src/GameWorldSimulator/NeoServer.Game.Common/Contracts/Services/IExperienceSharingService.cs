using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IExperienceSharingService
{
    void Share(ICreature creature);
}