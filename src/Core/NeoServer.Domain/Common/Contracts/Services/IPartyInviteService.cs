using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IPartyInviteService
{
    void Invite(IPlayer player, IPlayer invited);
}