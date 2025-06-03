using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IPlayerSkullService
{
    void UpdatePlayerSkull(IPlayer aggressor);
    void UpdateSkullOnAttack(IPlayer aggressor, IPlayer victim);
}