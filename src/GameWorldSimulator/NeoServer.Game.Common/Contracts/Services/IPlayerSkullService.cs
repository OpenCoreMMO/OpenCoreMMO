using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IPlayerSkullService
{
    void UpdatePlayerSkull(IPlayer aggressor);
}